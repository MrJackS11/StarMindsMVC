using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarMindsMVC.Data;
using StarMindsMVC.Models;

namespace StarMindsMVC.Controllers;

[Authorize]
public class CitasController : Controller
{
    private readonly StarMindsContext _context;

    public CitasController(StarMindsContext context)
    {
        _context = context;
    }

    // =======================================================
    // 1. MIS CITAS: Vista del estudiante (Anonimato protegido)
    // =======================================================
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var estudiante = await ObtenerEstudianteActualAsync();
        if (estudiante == null)
        {
            return Forbid();
        }

        var citas = await _context.Citas
            .Include(c => c.Psicologo)
            .Where(c => c.EstudianteId == estudiante.Id)
            .OrderByDescending(c => c.FechaHoraInicio)
            .ToListAsync();

        return View(citas);
    }

    // =======================================================
    // 2. AGENDAR CITA: Selección de Terapeuta y Horarios (HU03)
    // =======================================================
    [HttpGet]
    public async Task<IActionResult> Agendar(int? psicologoId, DateTime? fecha)
    {
        var psicologos = await _context.Psicologos.Where(p => p.Activo).ToListAsync();
        var fechaSeleccionada = fecha ?? DateTime.Today.AddDays(1);

        var model = new AgendarCitaViewModel
        {
            Fecha = fechaSeleccionada,
            PsicologosDisponibles = psicologos
        };

        if (psicologos.Any())
        {
            model.PsicologoId = psicologoId ?? psicologos.First().Id;
            model.HorariosDisponibles = await CalcularHorariosDisponiblesAsync(model.PsicologoId, model.Fecha);
        }

        return View(model);
    }

    // =======================================================
    // 3. CONFIRMAR RESERVA (HU03 + HU04: Bloqueo anti-solapamiento)
    // =======================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agendar(AgendarCitaViewModel model)
    {
        var estudiante = await ObtenerEstudianteActualAsync();
        if (estudiante == null) return Forbid();

        var psicologo = await _context.Psicologos.FindAsync(model.PsicologoId);
        if (psicologo == null)
        {
            ModelState.AddModelError("PsicologoId", "Especialista no válido.");
        }

        // Armar inicio y fin del bloque de 45 minutos (HU04)
        DateTime inicio = model.Fecha.Date.Add(model.HoraInicio);
        DateTime fin = inicio.AddMinutes(45);

        // Validación anti-solapamiento: verifica si el psicólogo ya tiene cita en ese rango
        // Considerando que necesita 15 minutos de descanso posterior
        bool ocupado = await _context.Citas.AnyAsync(c =>
            c.PsicologoId == model.PsicologoId &&
            c.Estado != "Cancelada" &&
            c.FechaHoraInicio.Date == model.Fecha.Date &&
            c.FechaHoraInicio < fin.AddMinutes(15) &&
            c.FechaHoraFin > inicio);

        if (ocupado)
        {
            ModelState.AddModelError(string.Empty, "El horario seleccionado ya no está disponible. Elija otro bloque.");
        }

        if (!ModelState.IsValid)
        {
            model.PsicologosDisponibles = await _context.Psicologos.Where(p => p.Activo).ToListAsync();
            model.HorariosDisponibles = await CalcularHorariosDisponiblesAsync(model.PsicologoId, model.Fecha);
            return View(model);
        }

        var cita = new Cita
        {
            EstudianteId = estudiante.Id,
            PsicologoId = model.PsicologoId,
            FechaHoraInicio = inicio,
            FechaHoraFin = fin,
            Modalidad = model.Modalidad,
            Estado = "Pendiente",
            MotivoConsulta = model.MotivoConsulta,
            FechaRegistro = DateTime.Now
        };

        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();

        TempData["Exito"] = "Tu cita ha sido agendada con éxito bajo absoluta confidencialidad.";
        return RedirectToAction(nameof(Index));
    }

    // =======================================================
    // 4. CANCELAR CITA
    // =======================================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        var estudiante = await ObtenerEstudianteActualAsync();
        if (estudiante == null) return Forbid();

        var cita = await _context.Citas.FirstOrDefaultAsync(c => c.Id == id && c.EstudianteId == estudiante.Id);
        if (cita != null && cita.Estado == "Pendiente")
        {
            cita.Estado = "Cancelada";
            await _context.SaveChangesAsync();
            TempData["Exito"] = "La cita fue cancelada correctamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    // =======================================================
    // ALGORITMO HU04: 45 min sesión + 15 min pausa obligatoria
    // =======================================================
    private async Task<List<TimeSpan>> CalcularHorariosDisponiblesAsync(int psicologoId, DateTime fecha)
    {
        var psicologo = await _context.Psicologos.FindAsync(psicologoId);
        if (psicologo == null) return new List<TimeSpan>();

        var citasDelDia = await _context.Citas
            .Where(c => c.PsicologoId == psicologoId &&
                        c.Estado != "Cancelada" &&
                        c.FechaHoraInicio.Date == fecha.Date)
            .ToListAsync();

        var disponibles = new List<TimeSpan>();
        TimeSpan actual = psicologo.HoraInicio;

        // Itera mientras el turno completo (45 min) quepa antes del fin de la jornada
        while (actual.Add(TimeSpan.FromMinutes(45)) <= psicologo.HoraFin)
        {
            DateTime inicioBloque = fecha.Date.Add(actual);
            DateTime finBloque = inicioBloque.AddMinutes(45);

            // Comprueba si choca con alguna cita existente
            bool estaOcupado = citasDelDia.Any(c =>
                c.FechaHoraInicio < finBloque && c.FechaHoraFin > inicioBloque);

            if (!estaOcupado)
            {
                disponibles.Add(actual);
            }

            // Salto algorítmico obligatorio: 45 minutos de atención + 15 minutos de descanso = 60 minutos
            actual = actual.Add(TimeSpan.FromMinutes(60));
        }

        return disponibles;
    }

    private async Task<Estudiante?> ObtenerEstudianteActualAsync()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdStr, out int userId))
        {
            return await _context.Estudiantes.FirstOrDefaultAsync(e => e.UsuarioId == userId);
        }
        return null;
    }
}