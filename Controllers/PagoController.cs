using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using System.Threading.Tasks;

public class PagoController : Controller
{
    private readonly ApplicationDbContext _context;

    public PagoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Pago/Procesar?medicoId=1&motivo=Consulta&fechaHora=2024-01-01T10:00
    public IActionResult Procesar(int medicoId, string motivo, DateTime fechaHora)
    {
        // Esta vista mostrará el formulario de pago
        return View();
    }

    // GET: /Pago/Exitoso?citaId=123
    public IActionResult Exitoso(int citaId)
    {
        // Vista de confirmación
        return View();
    }

    // GET: /Pago/Fallido
    public IActionResult Fallido()
    {
        return View();
    }
}