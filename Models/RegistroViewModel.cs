using System.ComponentModel.DataAnnotations;

namespace StarMindsMVC.Models;

public class RegistroViewModel
{
    [Required(ErrorMessage = "El correo institucional es obligatorio")]
    [EmailAddress]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El alias es obligatorio para proteger tu identidad")]
    [StringLength(60, MinimumLength = 3)]
    public string Alias { get; set; } = string.Empty;

    public string? NombreReal { get; set; }
    public string? CodigoInstitucional { get; set; }
}