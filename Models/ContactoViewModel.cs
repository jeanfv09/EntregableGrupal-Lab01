using System.ComponentModel.DataAnnotations;

namespace Lab01_Grupo1.Models
{
    public class ContactoViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Formato de correo inválido")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        public string Mensaje { get; set; } = string.Empty;
    }
}
