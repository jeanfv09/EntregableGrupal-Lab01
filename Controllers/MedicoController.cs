using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using System.Linq;
using System.Threading.Tasks;

public class MedicoController : Controller
{
    private readonly ApplicationDbContext _context;

    public MedicoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Medico
    public async Task<IActionResult> Index()
    {
        var medicos = await _context.Medicos
            .Include(m => m.Usuario)
            .Include(m => m.Perfil)
            .Include(m => m.Telefonos)
            .ToListAsync();

        return View(medicos);
    }

    // GET: /Medico/Create
public IActionResult Create()
{
    var model = new MedicoCreateViewModel
    {
        Telefonos = new List<string> { "", "" } // dos campos por defecto
    };
    return View(model);
}

    // POST: /Medico/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MedicoCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // Validar duplicados
        if (await _context.Usuarios.AnyAsync(u => u.Correo == model.Correo))
        {
            ModelState.AddModelError("Correo", "El correo ya está registrado.");
            return View(model);
        }

        if (await _context.Usuarios.AnyAsync(u => u.UsuarioNombre == model.UsuarioNombre))
        {
            ModelState.AddModelError("UsuarioNombre", "El nombre de usuario ya existe.");
            return View(model);
        }

        // 1. Crear Usuario con rol "medico"
        var usuario = new Usuario
        {
            UsuarioNombre = model.UsuarioNombre ?? string.Empty,
            Clave = model.Clave ?? string.Empty,
            Nombre = model.Nombre ?? string.Empty,
            Correo = model.Correo ?? string.Empty,
            Rol = "medico"
        };
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // 2. Crear Medico
        var medico = new Medico
        {
            IdUsuario = usuario.IdUsuario,
            Especialidad = model.Especialidad ?? string.Empty
        };
        _context.Medicos.Add(medico);
        await _context.SaveChangesAsync();

        // 3. Crear Perfil (solo si hay datos)
        if (!string.IsNullOrWhiteSpace(model.Universidad) ||
            !string.IsNullOrWhiteSpace(model.Experiencia) ||
            !string.IsNullOrWhiteSpace(model.Idiomas))
        {
            var perfil = new PerfilMedico
            {
                IdMedico = medico.IdMedico,
                Universidad = model.Universidad ?? string.Empty,
                PaisFormacion = model.PaisFormacion ?? string.Empty,
                Egreso = model.Egreso ?? string.Empty,
                Experiencia = model.Experiencia ?? string.Empty,
                Idiomas = model.Idiomas ?? string.Empty,
                TipoContrato = model.TipoContrato ?? string.Empty,
                TurnoPreferido = model.TurnoPreferido ?? string.Empty
            };
            _context.PerfilesMedico.Add(perfil);
        }

        // 4. Crear Teléfonos (solo si hay lista)
        if (model.Telefonos != null)
        {
            foreach (var tel in model.Telefonos.Where(t => !string.IsNullOrWhiteSpace(t)))
            {
                _context.TelefonosMedico.Add(new TelefonoMedico
                {
                    IdMedico = medico.IdMedico,
                    Telefono = tel
                });
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
