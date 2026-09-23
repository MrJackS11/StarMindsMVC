using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StarMindsMVC.Models;

public class Psicologo
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UsuarioId { get; set; }

    [ForeignKey("UsuarioId")]
    public Usuario? Usuario { get; set; }

    [Required(ErrorMessage = "El nombre del especialista es obligatorio")]
    [MaxLength(150)]
    public string NombreCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "La especialidad es obligatoria")]
    [MaxLength(100)]
    public string Especialidad { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? EnlaceVideollamada { get; set; } // Enlace estático (Meet/Zoom/Jitsi)

    public bool Activo { get; set; } = true;

    // Horario laboral estándar para el algoritmo de generación de bloques
    public TimeSpan HoraInicio { get; set; } = new TimeSpan(8, 0, 0);  // 08:00
    public TimeSpan HoraFin { get; set; } = new TimeSpan(12, 0, 0);    // 12:00

    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
}