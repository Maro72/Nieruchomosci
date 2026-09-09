[CmdletBinding()]
param(
    # Host name used by the IIS site and included in the certificate SAN.
    [string]$HostName = "localhost",

    # Existing IIS site which will receive the HTTPS binding.
    [string]$SiteName = "Mieszkaniec",

    # HTTPS port used by the IIS binding.
    [int]$HttpsPort = 443,

    # Certificate lifetime in days. Replace the self-signed certificate with a CA certificate in production.
    [int]$CertificateLifetimeDays = 825,

    # Also disable TLS 1.0 and TLS 1.1 at the Windows Schannel server level.
    [switch]$DisableLegacyTls
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)) {
    throw "Uruchom PowerShell jako Administrator."
}

Import-Module WebAdministration

$certificateSubject = "CN=$HostName"
$existingCertificate = Get-ChildItem Cert:\LocalMachine\My |
    Where-Object { $_.Subject -eq $certificateSubject -and $_.NotAfter -gt (Get-Date).AddDays(30) } |
    Sort-Object NotAfter -Descending |
    Select-Object -First 1

if ($null -eq $existingCertificate) {
    # Certificate is created in the local machine store so IIS can use its private key.
    $certificate = New-SelfSignedCertificate `
        -DnsName $HostName `
        -CertStoreLocation "Cert:\LocalMachine\My" `
        -Subject $certificateSubject `
        -FriendlyName "Mieszkaniec IIS TLS 1.3" `
        -KeyAlgorithm RSA `
        -KeyLength 3072 `
        -HashAlgorithm SHA256 `
        -KeyExportPolicy Exportable `
        -NotAfter (Get-Date).AddDays($CertificateLifetimeDays) `
        -Type SSLServerAuthentication
} else {
    $certificate = $existingCertificate
}

$bindingPath = "IIS:\SslBindings\0.0.0.0!$HttpsPort"
if (-not (Test-Path $bindingPath)) {
    New-WebBinding -Name $SiteName -Protocol https -Port $HttpsPort -IPAddress "*" -HostHeader $HostName
}

# Bind the certificate thumbprint to the HTTPS endpoint.
if (Test-Path $bindingPath) {
    Remove-Item $bindingPath -Force
}
New-Item $bindingPath -Thumbprint $certificate.Thumbprint -SSLFlags 1 | Out-Null

# Enable TLS 1.3 for Windows Schannel. This requires a supported Windows version.
$tls13Path = "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\TLS 1.3\Server"
New-Item $tls13Path -Force | Out-Null
New-ItemProperty $tls13Path -Name Enabled -PropertyType DWord -Value 1 -Force | Out-Null
New-ItemProperty $tls13Path -Name DisabledByDefault -PropertyType DWord -Value 0 -Force | Out-Null

if ($DisableLegacyTls) {
    # Optional hardening: disable TLS 1.0 and TLS 1.1 only after compatibility testing.
    foreach ($protocol in @("TLS 1.0", "TLS 1.1")) {
        $path = "HKLM:\SYSTEM\CurrentControlSet\Control\SecurityProviders\SCHANNEL\Protocols\$protocol\Server"
        New-Item $path -Force | Out-Null
        New-ItemProperty $path -Name Enabled -PropertyType DWord -Value 0 -Force | Out-Null
        New-ItemProperty $path -Name DisabledByDefault -PropertyType DWord -Value 1 -Force | Out-Null
    }
}

Write-Host "Certificate thumbprint: $($certificate.Thumbprint)"
Write-Host "IIS site: $SiteName"
Write-Host "HTTPS binding: https://$HostName`:$HttpsPort"
Write-Host "TLS 1.3 Schannel server setting enabled. Restart IIS or the server to apply it."
