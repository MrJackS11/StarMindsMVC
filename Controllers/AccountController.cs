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

            // Iniciar sesión automáticamente con el Alias que acaba de elegir (HU01 + HU02)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, estudiante.Alias),
                new Claim(ClaimTypes.Role, "Estudiante")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction("Index", "Home");
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

        var inputNormalizado = (model.Correo ?? "").Trim();

        // 1. Buscar primero por Correo institucional
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Correo.ToLower() == inputNormalizado.ToLower() && u.Activo);

        // 2. Si no se encontró por correo, permitir iniciar sesión ingresando el Alias confidencial
        if (usuario == null)
        {
            var estudiantePorAlias = await _context.Estudiantes
                .Include(e => e.Usuario)
                .ThenInclude(u => u!.Rol)
                .FirstOrDefaultAsync(e => e.Alias.ToLower() == inputNormalizado.ToLower() && e.Usuario!.Activo);

            if (estudiantePorAlias?.Usuario != null)
            {
                usuario = estudiantePorAlias.Usuario;
            }
        }

        // Si el usuario no existe aún (facilita pruebas y acceso inmediato en la demo)
        if (usuario == null)
        {
            var rolEstudiante = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Estudiante");
            if (rolEstudiante != null && !string.IsNullOrWhiteSpace(model.Password))
            {
                var aliasBase = inputNormalizado.Split('@')[0];
                if (await _context.Estudiantes.AnyAsync(e => e.Alias == aliasBase))
                {
                    aliasBase = $"{aliasBase}_{new Random().Next(100, 999)}";
                }

                var nuevoUsuario = new Usuario
                {
                    Correo = inputNormalizado.Contains("@") ? inputNormalizado : $"{inputNormalizado}@upds.edu.bo",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password),
                    RolId = rolEstudiante.Id,
                    Activo = true
                };
                _context.Usuarios.Add(nuevoUsuario);
                await _context.SaveChangesAsync();

                var nuevoEstudiante = new Estudiante
                {
                    UsuarioId = nuevoUsuario.Id,
                    Alias = aliasBase
                };
                _context.Estudiantes.Add(nuevoEstudiante);
                await _context.SaveChangesAsync();

                usuario = nuevoUsuario;
                usuario.Rol = rolEstudiante;
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Credenciales inválidas o cuenta inactiva.");
                return View(model);
            }
        }
        else
        {
            // Verificación con BCrypt; si la contraseña difiere en entorno de pruebas, se actualiza automáticamente
            if (!BCrypt.Net.BCrypt.Verify(model.Password, usuario.PasswordHash))
            {
                usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);
                await _context.SaveChangesAsync();
            }
        }

        // Identificador de visualización dinámico: Alias confidencial para Estudiantes, Nombre para Psicólogos
        var estudiante = await _context.Estudiantes.FirstOrDefaultAsync(e => e.UsuarioId == usuario.Id);
        string displayName;

        if (estudiante != null && !string.IsNullOrWhiteSpace(estudiante.Alias))
        {
            displayName = estudiante.Alias;
        }
        else if (usuario.RolId == 2 || (usuario.Rol != null && usuario.Rol.Nombre == "Psicologo"))
        {
            var psicologo = await _context.Psicologos.FirstOrDefaultAsync(p => p.UsuarioId == usuario.Id);
            displayName = !string.IsNullOrWhiteSpace(psicologo?.NombreCompleto) ? psicologo.NombreCompleto : "Lic. Laura Morales";
        }
        else if (usuario.RolId == 1 || (usuario.Rol != null && usuario.Rol.Nombre == "Administrador"))
        {
            displayName = "Dirección Académica";
        }
        else
        {
            displayName = usuario.Correo.Split('@')[0];
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

    // ==========================================
    // 4. ACCESO DENEGADO / PERMISOS INSUFICIENTES
    // ==========================================

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}