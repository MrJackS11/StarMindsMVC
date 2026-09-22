namespace StarMindsMVC.Models;

public class Estudiante
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string Alias { get; set; } = string.Empty;
    public string? NombreReal { get; set; }
    public string? CodigoInstitucional { get; set; }

    public Usuario? Usuario { get; set; }
}