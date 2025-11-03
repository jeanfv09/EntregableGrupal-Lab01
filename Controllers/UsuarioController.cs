using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using System.Threading.Tasks;
using System.Linq;
using System;

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
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string correo, string contrasena)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u =>
                (u.Correo == correo || u.UsuarioNombre == correo) &&
                u.Clave == contrasena);

        if (usuario != null)
        {
            // ✅ Guarda el nombre y rol en sesión
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? usuario.UsuarioNombre ?? "Usuario");
            HttpContext.Session.SetString("UsuarioRol", usuario.Rol ?? "usuario");

            TempData["Bienvenida"] = $"Bienvenido, {usuario.Nombre}";

            switch (usuario.Rol?.ToLower())
            {
                case "administrador":
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

            // ✅ Guarda el nombre en sesión al registrarse
            HttpContext.Session.SetString("UsuarioNombre", usuario.Nombre ?? usuario.UsuarioNombre ?? "Usuario");

            return RedirectToAction("Index", "Usuarios");
        }

        return View(usuario);
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
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

        return RedirectToAction("ProcesarPago", "Pago", new
        {
            medicoId = model.IdMedico,
            motivo = model.MotivoConsulta,
            fechaHora = model.FechaHora
        });
    }

    // ✅ CONTACTO (registro en BD)
    [HttpGet]
    public IActionResult Contacto()
    {
        return View();
    }
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Contacto(ContactoViewModel model)
{
    if (!ModelState.IsValid)
        return View(model);

    // declarar contacto en scope del método
    var contacto = new Contacto
    {
        Nombre = model.Nombre,
        Correo = model.Correo,
        Mensaje = model.Mensaje,
        FechaEnviado = DateTime.Now
    };

    // declarar variables que usaremos fuera de bloques
    KeyValuePair<string, float> top = default;
    string? labelText = null;
    float? score = null;

    try
    {
        var sampleData = new MLModel.ModelInput { Comentario = contacto.Mensaje ?? string.Empty };
        var labeledScores = MLModel.PredictAllLabels(sampleData);

        // obtenemos top si existe
        top = labeledScores.FirstOrDefault();
        if (!top.Equals(default(KeyValuePair<string, float>)))
        {
            var labelKey = (top.Key ?? string.Empty).Trim();

            if (labelKey.Equals("1", StringComparison.OrdinalIgnoreCase)
                || labelKey.Equals("1.0", StringComparison.OrdinalIgnoreCase)
                || labelKey.Equals("Positivo", StringComparison.OrdinalIgnoreCase)
                || labelKey.Equals("Bueno", StringComparison.OrdinalIgnoreCase))
            {
                labelText = "Positivo";
            }
            else if (labelKey.Equals("0", StringComparison.OrdinalIgnoreCase)
                || labelKey.Equals("0.0", StringComparison.OrdinalIgnoreCase)
                || labelKey.Equals("Negativo", StringComparison.OrdinalIgnoreCase)
                || labelKey.Equals("Malo", StringComparison.OrdinalIgnoreCase))
            {
                labelText = "Negativo";
            }
            else
            {
                labelText = labelKey; // fallback
            }

            score = top.Value;
        }
    }
    catch (Exception ex)
    {
        // log explícito para diagnóstico
        Console.WriteLine($"ERROR ML Predict: {ex.Message}");
    }

    // Asignar siempre (puede ser null)
    contacto.Sentimiento = labelText;
    contacto.ScorePrediccion = score;

    // DIAGNÓSTICO: imprimir antes de guardar
    Console.WriteLine($"DEBUG to-save => Sentimiento:'{contacto.Sentimiento ?? "<null>"}', Score:{contacto.ScorePrediccion?.ToString() ?? "<null>"}");

    _context.Contactos.Add(contacto);
    var rows = await _context.SaveChangesAsync();
    Console.WriteLine($"DEBUG DB => SaveChangesAsync returned {rows}");

    // rellenar viewmodel para mostrar en la vista
    model.ResultadoSentimiento = contacto.Sentimiento;
    model.Confidence = contacto.ScorePrediccion;

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

