
using Braintree;

namespace Lab01_Grupo1.Services
{
    public class BraintreeService
    {
        private readonly IBraintreeGateway _gateway;

        public BraintreeService(IConfiguration config)
        {
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
        }

        public async Task<Result<Transaction>> ProcessPaymentAsync(decimal amount, string nonce, string orderId)
        {
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
        }
    }
}