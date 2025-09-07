using Lab01_Grupo1.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lab01_Grupo1.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Usuariosy
        public async Task<IActionResult> Index()
        {
            List<Usuario> lista = await _context.Usuario.ToListAsync();
            return View(lista);
        }
    }
}
