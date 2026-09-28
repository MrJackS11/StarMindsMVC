using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarMindsMVC.Data;
using StarMindsMVC.Models;

namespace StarMindsMVC.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class BlogApiController : ControllerBase
{
    private readonly StarMindsContext _context;

    public BlogApiController(StarMindsContext context)
    {
        _context = context;
    }

    // GET: api/BlogApi
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArticuloBlog>>> GetArticulos()
    {
        return await _context.ArticulosBlog.ToListAsync();
    }

    // GET: api/BlogApi/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ArticuloBlog>> GetArticulo(int id)
    {
        var articulo = await _context.ArticulosBlog.FindAsync(id);
        if (articulo == null) return NotFound();
        return articulo;
    }

    // POST: api/BlogApi
    [HttpPost]
    public async Task<ActionResult<ArticuloBlog>> PostArticulo(ArticuloBlog articulo)
    {
        articulo.FechaPublicacion = DateTime.Now;
        _context.ArticulosBlog.Add(articulo);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetArticulo), new { id = articulo.Id }, articulo);
    }

    // PUT: api/BlogApi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutArticulo(int id, ArticuloBlog articulo)
    {
        if (id != articulo.Id) return BadRequest();

        _context.Entry(articulo).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!_context.ArticulosBlog.Any(e => e.Id == id)) return NotFound();
            throw;
        }

        return NoContent();
    }

    // DELETE: api/BlogApi/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteArticulo(int id)
    {
        var articulo = await _context.ArticulosBlog.FindAsync(id);
        if (articulo == null) return NotFound();

        _context.ArticulosBlog.Remove(articulo);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}