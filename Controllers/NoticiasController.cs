using Microsoft.AspNetCore.Mvc;
using NoticiasMedicas.Services;
using System.Threading.Tasks;
using System;

namespace NoticiasMedicas.Controllers
{
    public class NoticiasController : Controller
    {
        private readonly MedicalNewsService _newsService;

        public NoticiasController(MedicalNewsService newsService)
        {
            _newsService = newsService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var articles = await _newsService.GetMedicalNewsAsync();
                return View(articles);
            }
            catch (Exception ex)
            {
                // Log del error
                Console.WriteLine($"Error en controlador: {ex.Message}");
                
                // Puedes pasar un mensaje de error a la vista si quieres
                ViewBag.Error = "No se pudieron cargar las noticias en este momento.";
                return View(new List<MedicalArticle>());
            }
        }
    }
}