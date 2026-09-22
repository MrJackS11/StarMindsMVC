using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StarMindsMVC.Data;
using StarMindsMVC.Models;

namespace StarMindsMVC.Controllers;

public class AccountController : Controller
{
    private readonly StarMindsContext _context;

    public AccountController(StarMindsContext context)
    {
        _context = context;
    }

    // ==========================================
    // 1. REGISTRO CONFIDENCIAL (HU02)
    // ==========================================

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegistroViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Validaciones de unicidad previas antes de interactuar con la base de datos (CP02)
        if (await _context.Usuarios.AnyAsync(u => u.Correo == model.Correo))
        {
            ModelState.AddModelError("Correo", "El correo institucional ya se encuentra registrado.");
            return View(model);
        }

        if (await _context.Estudiantes.AnyAsync(e => e.Alias == model.Alias))
        {
            ModelState.AddModelError("Alias", "Este alias ya está en uso. Por favor elija otro para proteger su identidad.");
            return View(model);
        }

        // Validación explícita de duplicidad del código estudiantil opcional
        if (!string.IsNullOrWhiteSpace(model.CodigoInstitucional) &&
            await _context.Estudiantes.AnyAsync(e => e.CodigoInstitucional == model.CodigoInstitucional))
        {
            ModelState.AddModelError("CodigoInstitucional", "Este código estudiantil ya se encuentra registrado.");
            return View(model);
        }

        var rolEstudiante = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Estudiante");
        if (rolEstudiante == null)
        {
            ModelState.AddModelError(string.Empty, "Error de configuración de roles en el sistema.");
            return View(model);
        }

        // Transacción atómica: se guardan ambas entidades o se revierte todo
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Persistencia de credenciales
            var usuario = new Usuario
            {
                Correo = model.Correo,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                RolId = rolEstudiante.Id,
                Activo = true
            };
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            // Persistencia del perfil confidencial
            var estudiante = new Estudiante
            {
                UsuarioId = usuario.Id,
                Alias = model.Alias,
                NombreReal = string.IsNullOrWhiteSpace(model.NombreReal) ? null : model.NombreReal,
                CodigoInstitucional = string.IsNullOrWhiteSpace(model.CodigoInstitucional) ? null : model.CodigoInstitucional
            };
            _context.Estudiantes.Add(estudiante);
            await _context.SaveChangesAsync();

            // Confirmación definitiva
            await transaction.CommitAsync();

            return RedirectToAction("Login", "Account");
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            ModelState.AddModelError(string.Empty, "Ocurrió un inconveniente al registrar la cuenta. Por favor intente nuevamente.");
            return View(model);
        }
    }

    // ==========================================
    // 2. INICIO DE SESIÓN CON COOKIES (HU01)
    // ==========================================

    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Correo == model.Correo && u.Activo);

        // Verificación de credenciales con BCrypt (CP03)
        if (usuario == null || !BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
        {
            ModelState.AddModelError(string.Empty, "Credenciales inválidas o cuenta inactiva.");
            return View(model);
        }

        // Obtener el Alias si es estudiante para exhibir únicamente su identificador anónimo (CP04)
        string displayName = usuario.Correo;
        if (usuario.Rol?.Nombre == "Estudiante")
        {
            var estudiante = await _context.Estudiantes.FirstOrDefaultAsync(e => e.UsuarioId == usuario.Id);
            if (estudiante != null)
            {
                displayName = estudiante.Alias;
            }
        }

        // Creación de Claims y sesión con Cookie
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, displayName),
            new Claim(ClaimTypes.Role, usuario.Rol?.Nombre ?? "Estudiante")
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.Recordarme,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        return RedirectToAction("Index", "Home");
    }

    // ==========================================
    // 3. CERRAR SESIÓN
    // ==========================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }
}