using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using System.Threading.Tasks;
using System.Linq;

namespace Lab01_Grupo1.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
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
            var usuarios = await _context.Usuarios
                .Where(u => u.Rol == "usuario")
                .ToListAsync();

            return View(usuarios);
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
