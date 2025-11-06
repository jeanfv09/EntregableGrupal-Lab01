using Microsoft.AspNetCore.Mvc;
using Lab01_Grupo1.Models;
using Lab01_Grupo1.Services;
using System;
using System.Threading.Tasks;

namespace Lab01_Grupo1.Controllers
{
    public class ContactoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MLModelService _mlService;

        public ContactoController(ApplicationDbContext context, MLModelService mlService)
        {
            _context = context;
            _mlService = mlService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // 🔹 Cargar la vista que realmente existe en Views/Usuarios/Contacto.cshtml
            return View("~/Views/Usuarios/Contacto.cshtml", new ContactoViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactoViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Usuarios/Contacto.cshtml", model);

            var contacto = new Contacto
            {
                Nombre = model.Nombre,
                Correo = model.Correo,
                Mensaje = model.Mensaje,
                FechaEnviado = DateTime.Now
            };

            try
            {
                var resultado = _mlService.AnalizarSentimiento(model.Mensaje);

                contacto.Sentimiento = resultado.Prediccion;
                contacto.ScorePrediccion = resultado.Confianza;

                _context.Contactos.Add(contacto);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "Gracias por tu mensaje. Tu sentimiento fue analizado correctamente.";

                // 🔹 Redirige a la página de gracias en la carpeta Usuarios
                return RedirectToAction("Gracias");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error al analizar sentimiento: {ex.Message}");
                TempData["Error"] = "Ocurrió un error al analizar tu mensaje.";
                return View("~/Views/Usuarios/Contacto.cshtml", model);
            }
        }

        [HttpGet]
        public IActionResult Gracias()
        {
            // 🔹 Cargar la vista de gracias que está en Usuarios
            return View("~/Views/Usuarios/Gracias.cshtml");
        }
    }
}

