using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarMindsMVC.Data;
using StarMindsMVC.Models;

namespace StarMindsMVC.Controllers;

[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    private readonly StarMindsContext _context;

    public AdminController(StarMindsContext context)
    {
        _context = context;
    }

    // GET: /Admin/Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var totalCitas = await _context.Citas.CountAsync();
        var citasCompletadas = await _context.Citas.CountAsync(c => c.Estado == "Completada");
        var citasNoAsistio = await _context.Citas.CountAsync(c => c.Estado == "No Asistio");
        var citasCanceladas = await _context.Citas.CountAsync(c => c.Estado == "Cancelada");
        var citasPendientes = await _context.Citas.CountAsync(c => c.Estado == "Pendiente" || c.Estado == "Confirmada");

        // Regla de Negocio acordada: (Citas Completadas / Total Citas) * 100
        double tasaAsistencia = totalCitas > 0 
            ? Math.Round(((double)citasCompletadas / totalCitas) * 100, 1) 
            : 0.0;

        ViewBag.TotalCitas = totalCitas;
        ViewBag.CitasCompletadas = citasCompletadas;
        ViewBag.CitasNoAsistio = citasNoAsistio;
        ViewBag.CitasCanceladas = citasCanceladas;
        ViewBag.CitasPendientes = citasPendientes;
        ViewBag.TasaAsistencia = tasaAsistencia;

        // Listado de especialistas para gestión
        var psicologos = await _context.Psicologos.ToListAsync();
        return View(psicologos);
    }
}