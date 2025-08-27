using Microsoft.AspNetCore.Mvc;
using Lab01_Grupal.Models;

namespace Lab01_Grupal.Controllers
{
    public class MedicaController : Controller
    {
        // GET: /Medica/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Medica/Login
        [HttpPost]
        public IActionResult Login(string correo, string contraseña)
        {
            // 🔹 Aquí luego validarás usuario/contraseña
            // Por ahora, redirigimos al Home directamente
            return RedirectToAction("Index", "Home");
        }

        // GET: /Medica/Register
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Medica/Register
        [HttpPost]
        public IActionResult Register(Paciente paciente)
        {
            if (ModelState.IsValid)
            {
                // 🔹 Aquí luego guardarás el paciente en la base de datos
                return RedirectToAction("Login");
            }

            return View(paciente);
        }
    }
}
