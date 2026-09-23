using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarMindsMVC.Models;

public class Cita
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int EstudianteId { get; set; }

    [ForeignKey("EstudianteId")]
    public Estudiante? Estudiante { get; set; }

    [Required]
    public int PsicologoId { get; set; }

    [ForeignKey("PsicologoId")]
    public Psicologo? Psicologo { get; set; }

    [Required]
    public DateTime FechaHoraInicio { get; set; }

    [Required]
    public DateTime FechaHoraFin { get; set; } // 45 minutos después

    [Required]
    [MaxLength(20)]
    public string Modalidad { get; set; } = "Presencial"; // "Presencial" o "Virtual"

    [Required]
    [MaxLength(20)]
    public string Estado { get; set; } = "Pendiente"; // Pendiente, Confirmada, En Curso, Completada, Cancelada, No Asistió

    [MaxLength(500)]
    public string? MotivoConsulta { get; set; }

    public DateTime FechaRegistro { get; set; } = DateTime.Now;
}