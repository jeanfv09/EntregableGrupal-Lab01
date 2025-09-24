using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using System.Threading.Tasks;
using System.Linq;

public class UsuariosController : Controller
{
    private readonly ApplicationDbContext _context;

    public UsuariosController(ApplicationDbContext context)
    {
        _context = context;
    }


    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string correo, string contrasena)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                (u.Correo == correo || u.UsuarioNombre == correo) &&
                u.Clave == contrasena);

        if (usuario != null)
        {
            TempData["Bienvenida"] = $"Bienvenido, {usuario.Nombre}";

            switch (usuario.Rol?.ToLower())
            {
                case "administrador":
                    return RedirectToAction("Index", "Admin");
                case "medico":
                    return RedirectToAction("Index", "Admin");
                default:
                    return RedirectToAction("Index", "Usuarios");
            }
        }

        ViewBag.Error = "Credenciales incorrectas. Intenta nuevamente.";
        return View();
    }

    [HttpGet]
    public IActionResult Medicos()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new Usuario());
    }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Register(Usuario usuario)
{
    if (ModelState.IsValid)
    {
        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        // Redirigir al Index de Medico
        return RedirectToAction("Index", "Medico");
    }

    return View(usuario);
}


    public async Task<IActionResult> Index()
    {
        var lista = await _context.Usuarios.ToListAsync();
        return View(lista);
    }

    [HttpGet]
    public IActionResult FormularioSalud()
    {
        return View(new FormularioSaludViewModel());
    }

    [HttpPost]
    public IActionResult FormularioSalud(FormularioSaludViewModel model)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("FormularioSaludEnviado");
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult FormularioSaludEnviado()
    {
        return View();
    }
[HttpGet]
public async Task<IActionResult> Citas(string? nombre, string? especialidad)
{
    var query = _context.Medicos
        .Include(m => m.Usuario)
        .Include(m => m.Perfil)
        .Include(m => m.Telefonos)
        .AsQueryable();

    if (!string.IsNullOrWhiteSpace(nombre))
        query = query.Where(m => m.Usuario.Nombre.Contains(nombre));

    if (!string.IsNullOrWhiteSpace(especialidad))
        query = query.Where(m => m.Especialidad == especialidad);

    var medicos = await query.ToListAsync();

    ViewBag.Especialidades = await _context.Medicos
        .Select(m => m.Especialidad)
        .Distinct()
        .ToListAsync();

    ViewData["NombreFiltro"] = nombre;
    ViewData["EspecialidadFiltro"] = especialidad;

    // 🔹 Aquí devolvemos un CitaViewModel, no la lista sola
    var model = new CitaViewModel
    {
        Medicos = medicos
    };

    return View(model);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AgendarCita(CitaViewModel model)
{
    if (!ModelState.IsValid)
    {
        model.Medicos = await _context.Medicos
            .Include(m => m.Usuario)
            .ToListAsync();
        return View("Citas", model);
    }

    var cita = new Cita
    {
        IdPaciente = 1, // ⚠️ aquí luego deberías usar el usuario logueado
        IdMedico = model.IdMedico,
        Especialidad = _context.Medicos.First(m => m.IdMedico == model.IdMedico).Especialidad,
        FechaHora = model.FechaHora,
        MotivoConsulta = model.MotivoConsulta,
        Estado = "pendiente",
        CodigoCita = Guid.NewGuid().ToString().Substring(0, 8)
    };

    _context.Citas.Add(cita);
    await _context.SaveChangesAsync();

    TempData["Mensaje"] = "Cita agendada correctamente.";
    return RedirectToAction("Citas");
}



    [HttpGet]
    public IActionResult Contacto()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Contacto(ContactoViewModel model)
    {
        if (ModelState.IsValid)
        {
            return RedirectToAction("Gracias");
        }

        return View(model);
    }
    
    [HttpGet]
    public IActionResult Gracias()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Usuario usuario)
    {
        if (ModelState.IsValid)
        {
            usuario.Rol = "medico"; 
            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var medico = new Medico
            {
                IdUsuario = usuario.IdUsuario
            };

            _context.Medicos.Add(medico);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        return View(usuario);
    }

}
