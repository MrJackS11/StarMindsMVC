using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarMindsMVC.Data;
using StarMindsMVC.Models;

namespace StarMindsMVC.Controllers;

[Authorize(Roles = "Psicologo")]
public class PsicologoController : Controller
{
    private readonly StarMindsContext _context;

    public PsicologoController(StarMindsContext context)
    {
        _context = context;
    }

    // GET: /Psicologo/Agenda
    public async Task<IActionResult> Agenda()
    {
        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(usuarioIdClaim)) return Challenge();

        int usuarioId = int.Parse(usuarioIdClaim);
        var psicologo = await _context.Psicologos.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
        if (psicologo == null) return Forbid();

        var citas = await _context.Citas
            .Include(c => c.Estudiante)
            .Where(c => c.PsicologoId == psicologo.Id)
            .OrderBy(c => c.FechaHoraInicio)
            .Select(c => new AgendaPsicologoViewModel
            {
                CitaId = c.Id,
                FechaHoraInicio = c.FechaHoraInicio,
                FechaHoraFin = c.FechaHoraFin,
                AliasEstudiante = c.Estudiante != null ? c.Estudiante.Alias : "Anónimo",
                Modalidad = c.Modalidad,
                Estado = c.Estado,
                EnlaceVideollamada = psicologo.EnlaceVideollamada,
                MotivoConsulta = c.MotivoConsulta,
                TieneNotaClinica = _context.NotasClinicas.Any(n => n.CitaId == c.Id)
            })
            .ToListAsync();

        ViewBag.NombrePsicologo = psicologo.NombreCompleto;
        return View(citas);
    }

    // POST: /Psicologo/CambiarEstado
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(int citaId, string nuevoEstado)
    {
        var cita = await _context.Citas.FindAsync(citaId);
        if (cita == null) return NotFound();

        cita.Estado = nuevoEstado;
        await _context.SaveChangesAsync();

        TempData["Exito"] = $"Estado de la sesión actualizado a: {nuevoEstado}";
        return RedirectToAction(nameof(Agenda));
    }

    // GET: /Psicologo/RedactarNota?citaId=5
    public async Task<IActionResult> RedactarNota(int citaId)
    {
        var cita = await _context.Citas
            .Include(c => c.Estudiante)
            .FirstOrDefaultAsync(c => c.Id == citaId);

        if (cita == null) return NotFound();

        var notaExistente = await _context.NotasClinicas.FirstOrDefaultAsync(n => n.CitaId == citaId);

        var model = new RegistrarNotaViewModel
        {
            CitaId = cita.Id,
            AliasEstudiante = cita.Estudiante?.Alias ?? "Anónimo",
            FechaCita = cita.FechaHoraInicio,
            Contenido = notaExistente?.Contenido ?? string.Empty
        };

        return View(model);
    }

    // POST: /Psicologo/RedactarNota
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RedactarNota(RegistrarNotaViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        int usuarioId = int.Parse(usuarioIdClaim!);
        var psicologo = await _context.Psicologos.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
        if (psicologo == null) return Forbid();

        var nota = await _context.NotasClinicas.FirstOrDefaultAsync(n => n.CitaId == model.CitaId);

        if (nota == null)
        {
            nota = new NotaClinica
            {
                CitaId = model.CitaId,
                PsicologoId = psicologo.Id,
                Contenido = model.Contenido,
                FechaRegistro = DateTime.Now
            };
            _context.NotasClinicas.Add(nota);
        }
        else
        {
            nota.Contenido = model.Contenido;
            nota.FechaRegistro = DateTime.Now;
        }

        await _context.SaveChangesAsync();
        TempData["Exito"] = "Nota clínica confidencial guardada correctamente.";
        return RedirectToAction(nameof(Agenda));
    }
}