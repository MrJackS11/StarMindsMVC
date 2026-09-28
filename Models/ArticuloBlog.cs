using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace StarMindsMVC.Models;

[Table("articulosblog")]
public class ArticuloBlog
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El contenido es obligatorio.")]
    public string Contenido { get; set; } = string.Empty;

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    [StringLength(50, ErrorMessage = "La categoría no puede superar los 50 caracteres.")]
    public string Categoria { get; set; } = string.Empty;

    [Required(ErrorMessage = "El identificador del autor es obligatorio.")]
    public int AutorId { get; set; }

    [ForeignKey("AutorId")]
    [JsonIgnore]
    public Usuario? Autor { get; set; }

    public DateTime FechaPublicacion { get; set; } = DateTime.Now;
}
