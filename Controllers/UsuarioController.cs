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

    // Mostrar formulario de login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string correo, string contrasena)
    {
        var usuario = await _context.Usuario
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
    public async Task<IActionResult> Register(Usuario usuario)
    {
        if (ModelState.IsValid)
        {
            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();
            return RedirectToAction("Login");
        }

        return View(usuario);
    }

    public async Task<IActionResult> Index()
    {
        var lista = await _context.Usuario.ToListAsync();
        return View(lista);
    }
    
    [HttpGet]
    public IActionResult FormularioSalud()
    {
        return View();
    }

    [HttpPost]
    public IActionResult FormularioSalud(string nombre, int edad, double peso, double altura, string sintomas, string antecedentes)
    {
        // Aquí podrías guardar en la base de datos si deseas
        // Ejemplo: guardar en una tabla FormularioSalud
        
        return RedirectToAction("FormularioSaludEnviado");
    }

    [HttpGet]
    public IActionResult FormularioSaludEnviado()
    {
        return View();
    }

}
