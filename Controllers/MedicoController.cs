using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using System.Threading.Tasks;
using System.Linq;

public class MedicoController : Controller
{
    private readonly ApplicationDbContext _context;

    public MedicoController(ApplicationDbContext context)
    {
        _context = context;
    }

 
    public async Task<IActionResult> Index()
    {
        var medicos = await _context.Medicos
            .Include(m => m.Usuario) 
            .ToListAsync();

        return View(medicos); 
    }
}
