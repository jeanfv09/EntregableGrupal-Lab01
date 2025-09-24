using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;


namespace Lab01_Grupal.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return RedirectToAction("Register", "Usuarios");
    }

    public async Task<IActionResult> Citas(string? nombre, string? especialidad)
    {
        var query = _context.Medicos
            .Include(m => m.Usuario)
            .Include(m => m.Perfil)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(nombre))
        {
            query = query.Where(m => m.Usuario.Nombre.Contains(nombre));
            ViewData["NombreFiltro"] = nombre;
        }

        if (!string.IsNullOrWhiteSpace(especialidad))
        {
            query = query.Where(m => m.Especialidad == especialidad);
            ViewData["EspecialidadFiltro"] = especialidad;
        }

        var especialidades = await _context.Medicos
            .Select(m => m.Especialidad)
            .Distinct()
            .ToListAsync();

        ViewBag.Especialidades = especialidades;

        var medicos = await query.ToListAsync();
        return View(medicos);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
