[CmdletBinding()]
param(
    # Application directory containing appsettings.json.
    [string]$ApplicationPath = (Split-Path -Parent $PSScriptRoot),

    # Destination directory for compressed database archives.
    [string]$BackupPath = (Join-Path (Split-Path -Parent $PSScriptRoot) "Backup"),

    # Supported schedule interval in days: 1, 7 or 14.
    [ValidateSet(1, 7, 14)]
    [int]$IntervalDays = 7,

    # Maximum number of archives kept in the Backup directory.
    [ValidateRange(1, 7)]
    [int]$MaxCopies = 7,

    # Path to mysqldump.exe on the Windows server.
    [string]$MySqlDumpPath = "C:\Program Files\MySQL\MySQL Server 9.7\bin\mysqldump.exe"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$appSettingsPath = Join-Path $ApplicationPath "appsettings.json"
$logPath = Join-Path $BackupPath "backup.log"

function Write-BackupLog {
    param([string]$Message)

    # Every operation is written to a simple text log for Task Scheduler diagnostics.
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $line = "[$timestamp] $Message"
    Add-Content -Path $logPath -Value $line -Encoding UTF8
    Write-Host $line
}

try {
    if (-not (Test-Path $appSettingsPath)) {
        throw "Configuration file not found: $appSettingsPath"
    }

    if (-not (Test-Path $MySqlDumpPath)) {
        throw "mysqldump.exe not found: $MySqlDumpPath"
    }

    # The backup directory is created automatically on the first run.
    New-Item -ItemType Directory -Path $BackupPath -Force | Out-Null

    # Read the existing connection string instead of duplicating database settings.
    $configuration = Get-Content -Path $appSettingsPath -Raw -Encoding UTF8 | ConvertFrom-Json
    $connectionString = $configuration.ConnectionStrings.DefaultConnection

    # Panel konfiguracji może nadpisać parametry przekazane domyślnie do skryptu.
    if ($null -ne $configuration.BackupSettings) {
        if ($configuration.BackupSettings.IntervalDays -in 1, 7, 14) {
            $IntervalDays = [int]$configuration.BackupSettings.IntervalDays
        }
        if (-not [string]::IsNullOrWhiteSpace($configuration.BackupSettings.BackupPath)) {
            $configuredPath = [string]$configuration.BackupSettings.BackupPath
            $BackupPath = if ([IO.Path]::IsPathRooted($configuredPath)) { $configuredPath } else { Join-Path $ApplicationPath $configuredPath }
        }
        if ($configuration.BackupSettings.MaxCopies -ge 1 -and $configuration.BackupSettings.MaxCopies -le 7) {
            $MaxCopies = [int]$configuration.BackupSettings.MaxCopies
        }
    }

    $logPath = Join-Path $BackupPath "backup.log"

    if ([string]::IsNullOrWhiteSpace($connectionString)) {
        throw "ConnectionStrings:DefaultConnection is missing"
    }

    # Parse the basic key=value parts of the MySQL connection string.
    $connection = @{}
    foreach ($part in ($connectionString -split ";")) {
        if ($part.Contains("=")) {
            $pair = $part.Split("=", 2)
            $connection[$pair[0].Trim().ToLowerInvariant()] = $pair[1].Trim()
        }
    }

    $server = if ($connection.ContainsKey("server")) { $connection["server"] } else { "127.0.0.1" }
    $port = if ($connection.ContainsKey("port")) { $connection["port"] } else { "3306" }
    $database = if ($connection.ContainsKey("database")) { $connection["database"] } else { throw "Database name is missing" }
    $user = if ($connection.ContainsKey("user")) { $connection["user"] } elseif ($connection.ContainsKey("uid")) { $connection["uid"] } else { throw "Database user is missing" }
    $password = if ($connection.ContainsKey("password")) { $connection["password"] } else { "" }

    # The timestamp makes every archive name unique and easy to identify.
    $stamp = Get-Date -Format "yyyy-MM-dd_HH-mm-ss"
    $sqlPath = Join-Path $BackupPath "mieszkaniec_$stamp.sql"
    $zipPath = Join-Path $BackupPath "mieszkaniec_$stamp.zip"

    Write-BackupLog "Starting database backup for $database on $server."

    # The password is passed only to mysqldump and is never written to the log.
    $dumpArguments = @(
        "--protocol=TCP",
        "--host=$server",
        "--port=$port",
        "--user=$user",
        "--password=$password",
        "--routines",
        "--events",
        "--triggers",
        "--single-transaction",
        "--result-file=$sqlPath",
        $database
    )

    & $MySqlDumpPath @dumpArguments
    if ($LASTEXITCODE -ne 0) {
        throw "mysqldump failed with exit code $LASTEXITCODE"
    }

    # Compress the SQL dump into one portable ZIP archive.
    Compress-Archive -Path $sqlPath -DestinationPath $zipPath -CompressionLevel Optimal -Force
    Remove-Item -Path $sqlPath -Force

    # Keep only the newest seven archives and remove older copies.
    $archives = @(Get-ChildItem -Path $BackupPath -Filter "mieszkaniec_*.zip" -File | Sort-Object LastWriteTime -Descending)
    if ($archives.Count -gt $MaxCopies) {
        $archives | Select-Object -Skip $MaxCopies | Remove-Item -Force
    }

    Write-BackupLog "Backup completed: $zipPath. Configured interval: $IntervalDays days."
}
catch {
    Write-BackupLog "ERROR: $($_.Exception.Message)"
    exit 1
}
