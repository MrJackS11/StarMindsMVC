using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarMindsMVC.Models;

public class NotaClinica
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int CitaId { get; set; }

    [ForeignKey("CitaId")]
    public Cita? Cita { get; set; }

    [Required]
    public int PsicologoId { get; set; }

    [ForeignKey("PsicologoId")]
    public Psicologo? Psicologo { get; set; }

    [Required(ErrorMessage = "El contenido de la nota es obligatorio.")]
    [StringLength(1000, ErrorMessage = "La nota clínica no puede superar los 1000 caracteres.")]
    public string Contenido { get; set; } = string.Empty;

    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}