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


    

    public IActionResult Medicos()
    {
        return RedirectToAction("Index", "Medico");
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
    // GET: /Medico/Edit/5
public async Task<IActionResult> Edit(int id)
{
    var medico = await _context.Medicos
        .Include(m => m.Usuario)
        .Include(m => m.Perfil)
        .Include(m => m.Telefonos)
        .FirstOrDefaultAsync(m => m.IdMedico == id);

    if (medico == null) return NotFound();

    var model = new MedicoCreateViewModel
    {
        UsuarioNombre = medico.Usuario.UsuarioNombre,
        Clave = medico.Usuario.Clave,
        Nombre = medico.Usuario.Nombre,
        Correo = medico.Usuario.Correo,
        Especialidad = medico.Especialidad,
        Universidad = medico.Perfil?.Universidad ?? "",
        PaisFormacion = medico.Perfil?.PaisFormacion ?? "",
        Egreso = medico.Perfil?.Egreso ?? "",
        Experiencia = medico.Perfil?.Experiencia ?? "",
        Idiomas = medico.Perfil?.Idiomas ?? "",
        TipoContrato = medico.Perfil?.TipoContrato ?? "planta",
        TurnoPreferido = medico.Perfil?.TurnoPreferido ?? "tarde",
        Telefonos = medico.Telefonos.Select(t => t.Telefono).ToList()
    };

    return View(model);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(int id, MedicoCreateViewModel model)
{
    if (!ModelState.IsValid) return View(model);

    var medico = await _context.Medicos
        .Include(m => m.Usuario)
        .Include(m => m.Perfil)
        .Include(m => m.Telefonos)
        .FirstOrDefaultAsync(m => m.IdMedico == id);

    if (medico == null) return NotFound();

    // Actualizar Usuario
    medico.Usuario.Nombre = model.Nombre;
    medico.Usuario.UsuarioNombre = model.UsuarioNombre;
    if (!string.IsNullOrWhiteSpace(model.Clave) && model.Clave != "****")
    {
        medico.Usuario.Clave = model.Clave;
    }

    medico.Usuario.Correo = model.Correo;

    // Actualizar Médico
    medico.Especialidad = model.Especialidad;

    // Perfil
    if (medico.Perfil == null)
        medico.Perfil = new PerfilMedico { IdMedico = medico.IdMedico };

    medico.Perfil.Universidad = model.Universidad;
    medico.Perfil.PaisFormacion = model.PaisFormacion;
    medico.Perfil.Egreso = model.Egreso;
    medico.Perfil.Experiencia = model.Experiencia;
    medico.Perfil.Idiomas = model.Idiomas;
    medico.Perfil.TipoContrato = model.TipoContrato.ToLower();
    medico.Perfil.TurnoPreferido = model.TurnoPreferido.ToLower();

    // Teléfonos
    _context.TelefonosMedico.RemoveRange(medico.Telefonos);
    medico.Telefonos = model.Telefonos
        .Where(t => !string.IsNullOrWhiteSpace(t))
        .Select(t => new TelefonoMedico { IdMedico = medico.IdMedico, Telefono = t })
        .ToList();

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}


// GET: /Medico/Delete/5
public async Task<IActionResult> Delete(int id)
{
    var medico = await _context.Medicos
        .Include(m => m.Usuario)
        .FirstOrDefaultAsync(m => m.IdMedico == id);

    if (medico == null) return NotFound();

    return View(medico);
}

[HttpPost, ActionName("Delete")]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteConfirmed(int id)
{
    var medico = await _context.Medicos
        .Include(m => m.Usuario)
        .Include(m => m.Perfil)
        .Include(m => m.Telefonos)
        .FirstOrDefaultAsync(m => m.IdMedico == id);

    if (medico == null) return NotFound();

    if (medico.Perfil != null)
        _context.PerfilesMedico.Remove(medico.Perfil);

    if (medico.Telefonos.Any())
        _context.TelefonosMedico.RemoveRange(medico.Telefonos);

    _context.Usuarios.Remove(medico.Usuario);
    _context.Medicos.Remove(medico);

    await _context.SaveChangesAsync();
    return RedirectToAction(nameof(Index));
}


}
