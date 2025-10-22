namespace Lab01_Grupo1.Data
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string SessionId { get; set; } = string.Empty; // Identifica la sesión del usuario
        public string Message { get; set; } = string.Empty;   // Contenido del mensaje
        public string Sender { get; set; } = string.Empty;    // "User" o "Bot"
        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Fecha y hora del mensaje
    }
}
