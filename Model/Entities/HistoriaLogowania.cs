using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mieszkaniec.Model.Entities;

public class HistoriaLogowania
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Uzytkownik { get; set; } = string.Empty;

    [Column(TypeName = "datetime")]
    public DateTime DataLogowania { get; set; }
}
