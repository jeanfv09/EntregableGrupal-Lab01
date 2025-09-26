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

    }
}
