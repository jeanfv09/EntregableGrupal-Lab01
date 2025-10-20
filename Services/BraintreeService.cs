using Braintree;
using System.Threading.Tasks;

namespace Lab01_Grupo1.Services
{
    // Nota: IConfiguration ya no es necesario aquí, ya que el IBraintreeGateway
    // está completamente configurado en Program.cs.
    public class BraintreeService
    {
        private readonly IBraintreeGateway _gateway;

        // Constructor CORREGIDO: Recibe la instancia IBraintreeGateway ya configurada
        public BraintreeService(IBraintreeGateway gateway) 
        {
            _gateway = gateway;
        }

        // El método GetGateway() se ha eliminado ya que no es una práctica común
        // exponer el gateway a otros servicios; la lógica de pago debería estar aquí.
        // Si necesitas el gateway en un controlador, inyecta BraintreeService.

        public string GenerateClientToken()
        {
            // Este método genera un token para el cliente, que luego usará
            // para enviar el 'nonce' al servidor.
            return _gateway.ClientToken.Generate();
        }

        public async Task<Result<Transaction>> ProcessPaymentAsync(decimal amount, string nonce, string orderId)
        {
            var request = new TransactionRequest
            {
                Amount = amount,
                PaymentMethodNonce = nonce,
                OrderId = orderId,
                Options = new TransactionOptionsRequest
                {
                    // Indica que se envíe inmediatamente para liquidación (cargo final)
                    SubmitForSettlement = true 
                }
            };

            // Se usa SaleAsync para procesar la transacción de venta.
            return await _gateway.Transaction.SaleAsync(request);
        }
    }
}