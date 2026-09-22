using Microsoft.EntityFrameworkCore;
using StarMindsMVC.Models;

namespace StarMindsMVC.Data;

public class StarMindsContext : DbContext
{
    public StarMindsContext(DbContextOptions<StarMindsContext> options) : base(options) { }

    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Estudiante> Estudiantes { get; set; }
}