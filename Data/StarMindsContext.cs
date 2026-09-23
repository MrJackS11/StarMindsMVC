using Microsoft.EntityFrameworkCore;
using StarMindsMVC.Models;

namespace StarMindsMVC.Data;

public class StarMindsContext : DbContext
{
    public StarMindsContext(DbContextOptions<StarMindsContext> options) : base(options) { }

    public DbSet<Rol> Roles { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Estudiante> Estudiantes { get; set; }
    public DbSet<Psicologo> Psicologos { get; set; }
    public DbSet<Cita> Citas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Evitar borrados en cascada accidentales en citas
        modelBuilder.Entity<Cita>()
            .HasOne(c => c.Estudiante)
            .WithMany()
            .HasForeignKey(c => c.EstudianteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Cita>()
            .HasOne(c => c.Psicologo)
            .WithMany(p => p.Citas)
            .HasForeignKey(c => c.PsicologoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}