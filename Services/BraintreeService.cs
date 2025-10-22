using Braintree;

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
                    SubmitForSettlement = true
                }
            };

            return await _gateway.Transaction.SaleAsync(request);
        }
    }
}
