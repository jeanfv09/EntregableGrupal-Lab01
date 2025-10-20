
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
<<<<<<< HEAD
            // Para desarrollo - simular Braintree sin credenciales reales
            _gateway = CreateMockGateway();
        }

        private IBraintreeGateway CreateMockGateway()
        {
            // Gateway simulado para desarrollo
            return new BraintreeGateway
            {
                Environment = Braintree.Environment.SANDBOX,
                MerchantId = "development_merchant_id",
                PublicKey = "development_public_key", 
                PrivateKey = "development_private_key"
            };
        }

        public string GenerateClientToken()
        {
            try
            {
                // Intentar generar token real
                return _gateway.ClientToken.Generate();
            }
            catch
            {
                // Token de desarrollo para testing
                return "sandbox_development_token_" + Guid.NewGuid().ToString().Substring(0, 10);
            }
=======
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
>>>>>>> feature/paypal-integration
        }

        public async Task<Result<Transaction>> ProcessPaymentAsync(decimal amount, string nonce, string orderId)
        {
<<<<<<< HEAD
            // Simular pago exitoso para desarrollo
            await Task.Delay(500);
            
            return new Result<Transaction> 
            { 
                Target = new Transaction 
                { 
                    Id = "dev_txn_" + Guid.NewGuid().ToString().Substring(0, 8),
                    Status = TransactionStatus.SETTLED,
                    Amount = amount
                },
                IsSuccess = () => true
            };
=======
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
>>>>>>> feature/paypal-integration
        }
    }
}