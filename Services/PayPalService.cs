using PayPal.Core;
using PayPal.v1.Payments;

public class PayPalService
{
    private readonly PayPalHttpClient _client;

    public PayPalService(IConfiguration config)
    {
        var environment = new SandboxEnvironment(
            config["PayPal:ClientId"], 
            config["PayPal:ClientSecret"]
        );
        _client = new PayPalHttpClient(environment);
    }

    public async Task<string> CreatePaymentAsync(decimal amount, string returnUrl, string cancelUrl, string description)
    {
        var payment = new Payment
        {
            Intent = "sale",
            Transactions = new List<Transaction>
            {
                new Transaction
                {
                    Amount = new Amount
                    {
                        Total = amount.ToString("0.00"),
                        Currency = "USD"
                    },
                    Description = description
                }
            },
            RedirectUrls = new RedirectUrls
            {
                ReturnUrl = returnUrl,
                CancelUrl = cancelUrl
            },
            Payer = new Payer { PaymentMethod = "paypal" }
        };

        var request = new PaymentCreateRequest();
        request.RequestBody(payment);

        var response = await _client.Execute(request);
        var result = response.Result<Payment>();

        return result.Links.First(x => x.Rel == "approve").Href;
    }

    public async Task<bool> ExecutePaymentAsync(string paymentId, string payerId)
    {
        var paymentExecution = new PaymentExecution { PayerId = payerId };
        var request = new PaymentExecuteRequest(paymentId);
        request.RequestBody(paymentExecution);

        try
        {
            var response = await _client.Execute(request);
            return response.Result<Payment>().State == "approved";
        }
        catch
        {
            return false;
        }
    }
}