using Microsoft.AspNetCore.Mvc;
using Braintree;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; 
using Lab01_Grupo1.Models; 
using Microsoft.Extensions.DependencyInjection; 
using System; // Aseguramos que System esté presente para Console.WriteLine

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IBraintreeGateway _braintreeGateway;
    private readonly IServiceProvider _serviceProvider; 

    public PaymentsController(IBraintreeGateway braintreeGateway, IServiceProvider serviceProvider)
    {
        _braintreeGateway = braintreeGateway;
        _serviceProvider = serviceProvider;
    }

    [HttpGet("client-token")]
    public IActionResult GetClientToken()
    {
        try
        {
            // ** CORRECCIÓN FINAL CS0117: Se elimina el objeto ClientTokenRequest. **
            // Dado que ninguna de las sintaxis de configuración de PayPal es reconocida por tu SDK (ClientTokenOptions, PayPalRequest, ClientTokenRequest.PayPal),
            // asumimos que tu SDK es muy antiguo y que la configuración de PayPal
            // debe realizarse exclusivamente en la inicialización de IBraintreeGateway.
            
            // Generamos el token sin pasar un objeto de solicitud.
            var clientToken = _braintreeGateway.ClientToken.Generate(); 
            
            return Ok(new { clientToken });
        }
        catch (Exception ex)
        {
            // Si falla, mostramos el error en la consola del servidor
            Console.WriteLine("Error generando Client Token: " + ex.Message);
            return BadRequest(new { error = "Error al generar token: " + ex.Message });
        }
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentProcessModel model)
    {
        try
        {
            // 1. Procesar pago con Braintree
            var request = new TransactionRequest
            {
                Amount = model.Amount,
                PaymentMethodNonce = model.Nonce,
                Options = new TransactionOptionsRequest
                {
                    SubmitForSettlement = true
                }
            };

            var result = await _braintreeGateway.Transaction.SaleAsync(request);

            if (result.IsSuccess())
            {
                // 2. Usar scope para obtener el DbContext
                using var scope = _serviceProvider.CreateScope();
                // NOTA: Si ApplicationDbContext no existe, cambiar por el nombre de tu DbContext.
                // Reemplaza ApplicationDbContext con el nombre de tu DbContext real si es diferente.
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); 

                // 3. Buscar la cita en la base de datos
                var cita = await context.Citas
                    .FirstOrDefaultAsync(c => c.IdCita == model.CitaId);

                if (cita == null)
                {
                    return BadRequest(new { 
                        success = false, 
                        error = "Cita no encontrada. ID: " + model.CitaId 
                    });
                }

                // Determinar el método de pago
                // CORRECCIÓN CS1929: Convertimos a string antes de usar Contains()
                string paymentTypeString = result.Transaction.PaymentInstrumentType.ToString();
                string metodoPago = paymentTypeString.Contains("PayPal") ? "PayPal" : "Braintree Card";

                // 4. Actualizar la cita con los datos del pago
                cita.EstadoPago = "pagado";
                cita.FechaPago = DateTime.Now;
                cita.TransactionId = result.Transaction.Id;
                cita.MetodoPago = metodoPago;
                cita.Estado = "confirmada";

                // 5. Guardar en la base de datos
                await context.SaveChangesAsync();

                return Ok(new { 
                    success = true, 
                    transactionId = result.Transaction.Id,
                    message = "¡Pago exitoso! Tu cita ha sido confirmada.",
                    citaId = cita.IdCita
                });
            }
            else
            {
                // El pago falló (ej. tarjeta rechazada)
                return BadRequest(new { 
                    success = false, 
                    error = result.Message 
                });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { 
                success = false, 
                error = "Error procesando pago: " + ex.Message 
            });
        }
    }
}

// Clase de modelo de proceso de pago (Asumiendo que esta clase existe y es correcta)
public class PaymentProcessModel
{
    // Eliminamos la advertencia CS8618 haciendo Nonce nullable
    public decimal Amount { get; set; }
    public string? Nonce { get; set; } // Permitimos que sea nulo si no se asigna al construir el modelo
    public int CitaId { get; set; } 
}