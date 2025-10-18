using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Lab01_Grupo1.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
         private readonly IDistributedCache _cache;
    private readonly ILogger<AdminController> _logger;
    private readonly TimeSpan _cacheTtl;
    private readonly JsonSerializerOptions _jsonOptions = new()  {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
    };

public AdminController(
        ApplicationDbContext context,
        IDistributedCache cache,
        ILogger<AdminController> logger,
        IConfiguration configuration)
    {
        _context = context;
        _cache = cache;
        _logger = logger;

        var ttlMinutes = configuration.GetValue<int>("Redis:CacheTtlMinutes", 2);
        _cacheTtl = TimeSpan.FromMinutes(Math.Min(ttlMinutes, 2));
    }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Powerbi()
        {
            return View();
        }

        // ✅ Nueva acción para listar citas
        [HttpGet]
        public async Task<IActionResult> ListaCitas()
        {
            var citas = await _context.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                    .ThenInclude(m => m.Usuario)
                .ToListAsync();

            return View(citas);
        }

        [HttpGet]
        public async Task<IActionResult> ListaUsuarios()
        {
            var cacheKey = "admin:usuarios:lista";

            try
            {
                // Intentar obtener desde cache
                var cached = await _cache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cached))
                {
                    _logger.LogInformation("Cache hit ListaUsuarios");
                    var cachedList = JsonSerializer.Deserialize<List<Usuario>>(cached, _jsonOptions)
                                     ?? new List<Usuario>();
                    return View(cachedList);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error leyendo cache en ListaUsuarios; proceder a DB");
            }

            // Cache miss -> consultar DB con proyección explícita para evitar entidades tracked/proxies
            var usuarios = await _context.Set<Usuario>()
                .Where(u => u.Rol == "usuario")
                .Select(u => new Usuario
                {
                    IdUsuario = u.IdUsuario,
                    UsuarioNombre = u.UsuarioNombre,
                    Nombre = u.Nombre,
                    Correo = u.Correo,
                    Rol = u.Rol
                    // No incluyas Clave por seguridad
                })
                .AsNoTracking()
                .ToListAsync();

            // Guardar en cache (JSON)
            try
            {
                var serialized = JsonSerializer.Serialize(usuarios, _jsonOptions);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _cacheTtl
                };
                await _cache.SetStringAsync(cacheKey, serialized, options);
                _logger.LogInformation("ListaUsuarios guardada en cache");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error guardando cache en ListaUsuarios");
            }

            return View(usuarios);
        }

    // ✅ Registro de Contactos
        [HttpGet]
        public async Task<IActionResult> RegistroContacto()
        {
            var contactos = await _context.Contactos
                .OrderByDescending(c => c.FechaEnviado)
                .ToListAsync();
            return View(contactos);
        }

        // GET: Editar Usuario
        [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Editar(int id, Lab01_Grupo1.Models.Usuario model)
{
    if (id != model.IdUsuario) return BadRequest();

    if (!ModelState.IsValid)
    {
        return View(model);
    }

    // Recuperar entidad completa desde la BDD
    var usuario = await _context.Usuarios
        .FirstOrDefaultAsync(u => u.IdUsuario == id);

    if (usuario == null) return NotFound();

    // Actualizar sólo campos permitidos (evita overposting)
    usuario.UsuarioNombre = model.UsuarioNombre;
    usuario.Nombre = model.Nombre;
    usuario.Correo = model.Correo;
    // Si quieres permitir cambiar rol desde la UI:
    usuario.Rol = model.Rol;

    // Si tienes un campo Clave y quieres permitir actualización con protección:
    if (!string.IsNullOrWhiteSpace(model.Clave) && model.Clave != "****")
    {
        usuario.Clave = model.Clave;
    }

    try
    {
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(ListaUsuarios));
    }
    catch (DbUpdateConcurrencyException)
    {
        if (!await _context.Usuarios.AnyAsync(e => e.IdUsuario == id)) return NotFound();
        throw;
    }
    catch (Exception ex)
    {
        // Evita que el fallo del logger escalone la excepción. Registra en consola durante depuración.
        Console.Error.WriteLine("Error al guardar Usuario: " + ex);
        ModelState.AddModelError("", "Ocurrió un error al guardar. Intenta nuevamente más tarde.");
        return View(model);
    }
}


    [HttpGet]
    public async Task<IActionResult> Eliminar(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario == null)
        {
            return NotFound();
        }
        return View(usuario);
    }

    // POST: Confirmar Eliminación
    [HttpPost, ActionName("Eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarConfirmado(int id)
    {
        var usuario = await _context.Usuarios.FindAsync(id);
        if (usuario != null)
        {
            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(ListaUsuarios));
    }
    }
}
