using System.ComponentModel.DataAnnotations;

namespace StarMindsMVC.Models;

public class AgendaPsicologoViewModel
{
    public int CitaId { get; set; }
    public DateTime FechaHoraInicio { get; set; }
    public DateTime FechaHoraFin { get; set; }
    public string AliasEstudiante { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string? EnlaceVideollamada { get; set; }
    public string? MotivoConsulta { get; set; }
    public bool TieneNotaClinica { get; set; }
}

public class RegistrarNotaViewModel
{
    public int CitaId { get; set; }
    public string AliasEstudiante { get; set; } = string.Empty;
    public DateTime FechaCita { get; set; }

    [Required(ErrorMessage = "Debe redactar la nota de seguimiento.")]
    [StringLength(1000, ErrorMessage = "Máximo 1000 caracteres.")]
    [Display(Name = "Nota Clínica Confidencial")]
    public string Contenido { get; set; } = string.Empty;
}