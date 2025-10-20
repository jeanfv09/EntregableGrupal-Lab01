using System.ComponentModel.DataAnnotations;

namespace Lab01_Grupo1.Models
{
    public class PaymentProcessModel
{
    // Eliminamos la advertencia CS8618 haciendo Nonce nullable
    public decimal Amount { get; set; }
    public string? Nonce { get; set; } // Permitimos que sea nulo si no se asigna al construir el modelo
    public int CitaId { get; set; } 
}

}
