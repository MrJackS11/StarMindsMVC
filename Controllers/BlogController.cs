using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarMindsMVC.Data;

namespace StarMindsMVC.Controllers;

public class BlogController : Controller
{
    private readonly StarMindsContext _context;

    public BlogController(StarMindsContext context)
    {
        _context = context;
    }

    // GET: /Blog
    // Acceso público sin requerir inicio de sesión
    public async Task<IActionResult> Index(string? categoria)
    {
        var query = _context.ArticulosBlog.AsQueryable();

        if (!string.IsNullOrEmpty(categoria))
        {
            query = query.Where(a => a.Categoria == categoria);
        }

        var articulos = await query.OrderByDescending(a => a.FechaPublicacion).ToListAsync();
        ViewBag.CategoriaSeleccionada = categoria;
        return View(articulos);
    }

    // GET: /Blog/Detalle/5
    public async Task<IActionResult> Detalle(int id)
    {
        var articulo = await _context.ArticulosBlog
            .Include(a => a.Autor)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (articulo == null) return NotFound();

        ViewBag.ArticulosRelacionados = await _context.ArticulosBlog
            .Where(a => a.Id != id)
            .OrderByDescending(a => a.FechaPublicacion)
            .Take(3)
            .ToListAsync();

        return View(articulo);
    }
}