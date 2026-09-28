using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using StarMindsMVC.Data;

namespace StarMindsMVC.Services;

/// <summary>
/// Transformación reactiva de Claims: Garantiza que en cada petición HTTP,
/// la identidad de la sesión refleje el Alias dinámico del estudiante registrado
/// o el nombre profesional del terapeuta, sin depender únicamente de cookies estáticas.
/// </summary>
public class StarMindsClaimsTransformation : IClaimsTransformation
{
    private readonly StarMindsContext _context;

    public StarMindsClaimsTransformation(StarMindsContext context)
    {
        _context = context;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
        {
            return principal;
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
        {
            // 1. Si es estudiante, el nombre SIEMPRE debe ser su Alias registrado en la BD
            var estudiante = await _context.Estudiantes.AsNoTracking().FirstOrDefaultAsync(e => e.UsuarioId == userId);
            if (estudiante != null && !string.IsNullOrWhiteSpace(estudiante.Alias))
            {
                var existingNameClaim = identity.FindFirst(ClaimTypes.Name);
                if (existingNameClaim == null || existingNameClaim.Value != estudiante.Alias)
                {
                    if (existingNameClaim != null)
                    {
                        identity.RemoveClaim(existingNameClaim);
                    }
                    identity.AddClaim(new Claim(ClaimTypes.Name, estudiante.Alias));
                }
                return principal;
            }

            // 2. Si es psicólogo, mostrar su Nombre Completo
            var psicologo = await _context.Psicologos.AsNoTracking().FirstOrDefaultAsync(p => p.UsuarioId == userId);
            if (psicologo != null && !string.IsNullOrWhiteSpace(psicologo.NombreCompleto))
            {
                var existingNameClaim = identity.FindFirst(ClaimTypes.Name);
                if (existingNameClaim == null || existingNameClaim.Value != psicologo.NombreCompleto)
                {
                    if (existingNameClaim != null)
                    {
                        identity.RemoveClaim(existingNameClaim);
                    }
                    identity.AddClaim(new Claim(ClaimTypes.Name, psicologo.NombreCompleto));
                }
                return principal;
            }

            // 3. Si es Administrador
            var usuario = await _context.Usuarios.Include(u => u.Rol).AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
            if (usuario?.Rol?.Nombre == "Administrador" || usuario?.RolId == 1)
            {
                var existingNameClaim = identity.FindFirst(ClaimTypes.Name);
                if (existingNameClaim == null || existingNameClaim.Value != "Dirección Académica")
                {
                    if (existingNameClaim != null)
                    {
                        identity.RemoveClaim(existingNameClaim);
                    }
                    identity.AddClaim(new Claim(ClaimTypes.Name, "Dirección Académica"));
                }
            }
        }

        return principal;
    }
}
