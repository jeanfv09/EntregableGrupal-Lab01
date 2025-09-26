using System;
using System.ComponentModel.DataAnnotations;

namespace Lab01_Grupo1.Models
{
    public class Contacto
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        // <-- Nueva propiedad que faltaba
        [Required(ErrorMessage = "El mensaje es obligatorio")]
        public string Mensaje { get; set; } = string.Empty;

        // Opcional: timestamp de envío
        public DateTime FechaEnviado { get; set; } = DateTime.UtcNow;
    }
}

