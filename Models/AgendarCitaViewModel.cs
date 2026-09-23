using System.ComponentModel.DataAnnotations;

namespace StarMindsMVC.Models;

public class AgendarCitaViewModel
{
    [Required(ErrorMessage = "Debe seleccionar un profesional")]
    public int PsicologoId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una fecha")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Debe seleccionar un horario disponible")]
    public TimeSpan HoraInicio { get; set; }

    [Required(ErrorMessage = "Seleccione la modalidad")]
    public string Modalidad { get; set; } = "Presencial"; // Presencial o Virtual

    [MaxLength(500, ErrorMessage = "El motivo no puede exceder los 500 caracteres")]
    public string? MotivoConsulta { get; set; }

    // Listas auxiliares para la interfaz gráfica
    public List<Psicologo>? PsicologosDisponibles { get; set; }
    public List<TimeSpan> HorariosDisponibles { get; set; } = new();
}