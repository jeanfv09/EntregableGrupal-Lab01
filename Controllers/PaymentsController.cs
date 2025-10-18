using Microsoft.AspNetCore.Mvc;
using Braintree;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // ← Agregar esto
using Lab01_Grupo1.Models; // ← Agregar esto
using Microsoft.Extensions.DependencyInjection; // ← Agregar esto

[ApiController]
[Route("api/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IBraintreeGateway _braintreeGateway;
    private readonly IServiceProvider _serviceProvider; // ← Agregar esto

    // ← Actualizar el constructor
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
            var clientToken = _braintreeGateway.ClientToken.Generate();
            return Ok(new { clientToken });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
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
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>(); // ← Cambia por tu DbContext

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

                // 4. Actualizar la cita con los datos del pago
                cita.EstadoPago = "pagado";
                cita.FechaPago = DateTime.Now;
                cita.TransactionId = result.Transaction.Id;
                cita.MetodoPago = "Braintree";
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

// ← Actualizar el modelo
public class PaymentProcessModel
{
    public decimal Amount { get; set; }
    public string Nonce { get; set; }
    public int CitaId { get; set; } // ← Agregar este campo
}