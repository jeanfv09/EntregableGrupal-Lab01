using Braintree;

namespace Lab01_Grupo1.Services
{
    public class BraintreeService
    {
        private readonly IBraintreeGateway _gateway;

        public BraintreeService(IConfiguration config)
        {
            _gateway = new BraintreeGateway
            {
                Environment = Braintree.Environment.SANDBOX,
                MerchantId = config["Braintree:MerchantId"],
                PublicKey = config["Braintree:PublicKey"],
                PrivateKey = config["Braintree:PrivateKey"]
            };
        }

        public IBraintreeGateway GetGateway()
        {
            return _gateway;
        }

        public string GenerateClientToken()
        {
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