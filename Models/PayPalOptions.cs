namespace Lab01_Grupo1.Configuration // Ajusta el namespace si es necesario
{
    public class PayPalOptions
    {
        public const string PayPal = "PayPal"; // Constante para la clave de la sección

        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string Mode { get; set; } = "sandbox";
    }
}