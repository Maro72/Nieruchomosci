using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Mieszkaniec.Model.Entities
{
    public class Najemca
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Nazwa firmy lub imię i nazwisko są wymagane.")]
        [StringLength(250)]
        public string NazwaFirmyOsoby { get; set; } = string.Empty;

        [StringLength(15)]
        public string? Nip { get; set; } // Zostawiam Twoją wielkość liter

        [StringLength(15)]
        public string? REGON { get; set; }

        [StringLength(200)]
        public string? Adres { get; set; }

        [Phone]
        [StringLength(50)]
        public string? Telefon { get; set; }

        [EmailAddress]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(100)]
        public string? OsobaKontaktowa { get; set; }

        // --- OBSŁUGA ARCHIWUM ---
        public bool CzyArchiwalny { get; set; } = false;
        public DateTime? DataArchiwizacji { get; set; }
        public string? Uwagi { get; set; }

        // --- RELACJE ---
        // Twoja relacja odwrotna do Lokali (zostaje bez zmian!)
        public List<LokalWynajem> WynajmowaneLokale { get; set; } = new();

        // Przyszła relacja do umów (odkomentujemy, jak będziemy robić klasę UmowaNajmu)
        // public List<UmowaNajmu> Umowy { get; set; } = new();
    }
}