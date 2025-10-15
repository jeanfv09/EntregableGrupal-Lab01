using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Options; // Necesario para IOptions
using Lab01_Grupo1.Configuration; // Necesario para PayPalOptions (Ajustar Namespace)
using Lab01_Grupo1.Services; // Necesario para PayPalService (Ajustar Namespace)

// Asegúrate de que el namespace de tu proyecto esté aquí si lo necesitas
// namespace Lab01_Grupo1.Controllers 

public class PagoController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly PayPalOptions _payPalOptions;
    private readonly PayPalService _payPalService; // Inyección del servicio de PayPal

    public PagoController(
        ApplicationDbContext context,
        IOptions<PayPalOptions> payPalOptions, // Nuevo parámetro
        PayPalService payPalService // Nuevo parámetro
    )
    {
        _context = context;
        _payPalOptions = payPalOptions.Value;
        _payPalService = payPalService;
    }

    // GET: /Pago/ProcesarPago?medicoId=1&motivo=Consulta&fechaHora=...
    public async Task<IActionResult> ProcesarPago(int medicoId, string motivo, DateTime fechaHora)
    {
        var medico = await _context.Medicos
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.IdMedico == medicoId);

        if (medico == null) return NotFound();

        // ⚠️ En un escenario real, aquí podrías verificar el precio
        var precioCita = 80.00m; 

        var model = new CitaPagoViewModel
        {
            MedicoId = medicoId,
            MedicoNombre = medico.Usuario.Nombre,
            Especialidad = medico.Especialidad,
            MotivoConsulta = motivo,
            FechaHora = fechaHora,
            Precio = precioCita 
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> IniciarPago(CitaPagoViewModel model) // Renombrado a IniciarPago
    {
        if (!ModelState.IsValid) return View("ProcesarPago", model);

        // 1. Crear la URL de retorno y cancelación para PayPal
        var returnUrl = Url.Action("ExecutePayment", "Pago", null, Request.Scheme);
        var cancelUrl = Url.Action("PaymentCancelled", "Pago", null, Request.Scheme);
        
        var description = $"Cita con Dr. {model.MedicoNombre} ({model.Especialidad})";

        try
        {
            // 2. Llamar al servicio de PayPal para crear la transacción
            var approvalUrl = await _payPalService.CreatePaymentAsync(
                model.Precio, 
                returnUrl, 
                cancelUrl, 
                description
            );

            // 3. Redirigir al usuario a PayPal para la aprobación
            return Redirect(approvalUrl);
        }
        catch (Exception ex)
        {
            // Manejo de errores
            TempData["Error"] = $"Error al iniciar el pago con PayPal: {ex.Message}";
            return RedirectToAction("Index", "Home");
        }
    }

    // Nueva acción: Captura la respuesta de éxito de PayPal y ejecuta el pago
    public async Task<IActionResult> ExecutePayment(string paymentId, string token, string PayerID)
    {
        if (string.IsNullOrEmpty(paymentId) || string.IsNullOrEmpty(PayerID))
        {
            TempData["Error"] = "Pago fallido: No se recibieron los datos de aprobación de PayPal.";
            return RedirectToAction("Citas", "Usuarios");
        }
        
        try
        {
            // 1. Ejecutar el pago final en PayPal
            var success = await _payPalService.ExecutePaymentAsync(paymentId, PayerID);

            if (success)
            {
                // 2. Si es exitoso, crear la cita en la DB
                // ⚠️ Aquí necesitarás la información de la CitaPagoViewModel. 
                // La mejor práctica es guardarla en una sesión o en la DB antes de la redirección.
                // Por simplicidad, simularemos los datos para crear la cita:
                
                var cita = new Cita
                {
                    IdPaciente = 1, // ⚠️ Reemplazar con el ID del usuario logueado
                    IdMedico = 1, // ⚠️ Reemplazar con el ID del médico de la cita original
                    Especialidad = "General", // ⚠️ Obtener de la sesión/DB
                    FechaHora = DateTime.Now.AddDays(1), // ⚠️ Obtener de la sesión/DB
                    MotivoConsulta = "Control", // ⚠️ Obtener de la sesión/DB
                    Estado = "confirmada",
                    CodigoCita = Guid.NewGuid().ToString().Substring(0, 8),
                    Precio = 80.00m,
                    EstadoPago = "pagado",
                    FechaPago = DateTime.Now,
                    MetodoPago = "PayPal",
                    TransactionId = paymentId // Guardar el ID de la transacción
                };

                _context.Citas.Add(cita);
                await _context.SaveChangesAsync();

                TempData["Mensaje"] = "¡Cita agendada y pagada correctamente con PayPal!";
                return RedirectToAction("Citas", "Usuarios");
            }
            else
            {
                TempData["Error"] = "Pago fallido: La transacción no fue aprobada por PayPal.";
                return RedirectToAction("Citas", "Usuarios");
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error al ejecutar el pago: {ex.Message}";
            return RedirectToAction("Citas", "Usuarios");
        }
    }

    // Nueva acción: Maneja la cancelación del pago por parte del usuario
    public IActionResult PaymentCancelled()
    {
        TempData["Advertencia"] = "El proceso de pago fue cancelado por el usuario.";
        return RedirectToAction("Citas", "Usuarios");
    }
}