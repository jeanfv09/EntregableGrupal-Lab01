using Microsoft.AspNetCore.Mvc;
using Lab01_Grupo1.Services;
using Lab01_Grupo1.Models;

namespace Lab01_Grupo1.Controllers
{
    [Route("assistant")]
    public class AssistantController : Controller
    {
        private readonly OllamaService _ollama;

        public AssistantController(OllamaService ollama)
        {
            _ollama = ollama;
        }

        // =====================================================
        //       MÉTODO CORREGIDO - usa AskOllamaAsync
        // =====================================================
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] AssistantRequest req)
        {
            try
            {
                string page = req.Page ?? "Página desconocida";

                // ✅ CORRECCIÓN: Usar AskOllamaAsync en lugar de AskOllamaOptimizedAsync
                string reply = await _ollama.AskOllamaAsync(req.Message, page);

                return Json(new { reply });
            }
            catch (Exception ex)
            {
                return Json(new { reply = $"❌ Error conectando a Ollama: {ex.Message}" });
            }
        }
    }
}
