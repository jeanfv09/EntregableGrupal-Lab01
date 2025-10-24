using Microsoft.AspNetCore.Mvc;
using Lab01_Grupo1.Services;

namespace Lab01_Grupo1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DrugInfoController : ControllerBase
    {
        private readonly OpenFDAService _fdaService;
        private readonly ILogger<DrugInfoController> _logger;

        public DrugInfoController(OpenFDAService fdaService, ILogger<DrugInfoController> logger)
        {
            _fdaService = fdaService;
            _logger = logger;
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchDrug([FromQuery] string drugName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(drugName))
                {
                    return BadRequest(new { error = "El parámetro 'drugName' es requerido" });
                }

                _logger.LogInformation("Buscando información para medicamento: {DrugName}", drugName);
                
                var information = await _fdaService.GetDrugInformation(drugName);
                
                return Ok(new { 
                    drug = drugName,
                    information = information,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar información del medicamento: {DrugName}", drugName);
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { 
                status = "Servicio de información médica funcionando",
                service = "OpenFDA Drug Information",
                timestamp = DateTime.UtcNow
            });
        }
    }
}

