using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using Lab01_Grupo1.Services;
using Braintree;

namespace Lab01_Grupo1.Controllers
{
    public class PagoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly Lab01_Grupo1.Services.BraintreeService _braintreeService; 

        public PagoController(ApplicationDbContext context, Lab01_Grupo1.Services.BraintreeService braintreeService)
        {
            _context = context;
            _braintreeService = braintreeService;
        }

        // GET: /Pago/ProcesarPago
        public async Task<IActionResult> ProcesarPago(int medicoId, string motivo, DateTime fechaHora)
        {
            var medico = await _context.Medicos
                .Include(m => m.Usuario)
                .FirstOrDefaultAsync(m => m.IdMedico == medicoId);

            if (medico == null) return NotFound();

            var clientToken = _braintreeService.GenerateClientToken();
            
            var model = new CitaPagoViewModel
            {
                MedicoId = medicoId,
                MedicoNombre = medico.Usuario.Nombre,
                Especialidad = medico.Especialidad,
                MotivoConsulta = motivo,
                FechaHora = fechaHora,
                Precio = 80.00m
            };

            ViewBag.ClientToken = clientToken;
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ProcesarPago(CitaPagoViewModel model, string paymentMethodNonce)
        {
            if (string.IsNullOrEmpty(paymentMethodNonce))
            {
                ModelState.AddModelError("", "Por favor completa la información de pago.");
                ViewBag.ClientToken = _braintreeService.GenerateClientToken();
                return View(model);
            }

            // Procesar pago con Braintree
            var result = await _braintreeService.ProcessPaymentAsync(
                model.Precio, 
                paymentMethodNonce, 
                $"CITA-{model.MedicoId}-{DateTime.Now:yyyyMMddHHmmss}"
            );

            if (result.IsSuccess())
            {
                // Pago exitoso - crear cita
                var cita = new Cita
                {
                    IdPaciente = 1, // Obtener del usuario logueado después
                    IdMedico = model.MedicoId,
                    Especialidad = model.Especialidad,
                    FechaHora = model.FechaHora,
                    MotivoConsulta = model.MotivoConsulta,
                    Estado = "confirmada",
                    CodigoCita = Guid.NewGuid().ToString().Substring(0, 8).ToUpper(),
                    Precio = model.Precio,
                    EstadoPago = "pagado",
                    FechaPago = DateTime.Now,
                    MetodoPago = "braintree",
                    TransactionId = result.Target.Id
                };

                _context.Citas.Add(cita);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "¡Cita agendada y pagada correctamente!";
                return RedirectToAction("Exitoso", new { citaId = cita.IdCita });
            }
            else
            {
                ModelState.AddModelError("", $"Error en el pago: {result.Message}");
                ViewBag.ClientToken = _braintreeService.GenerateClientToken();
                return View(model);
            }
        }

        public IActionResult Exitoso(int citaId)
        {
            var cita = _context.Citas
                .Include(c => c.Medico)
                .ThenInclude(m => m.Usuario)
                .FirstOrDefault(c => c.IdCita == citaId);

            if (cita == null) return NotFound();

            return View(cita);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}