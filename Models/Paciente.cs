using System.ComponentModel.DataAnnotations;

namespace Lab01_Grupal.Models
{
    public class Paciente
    {
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe contener exactamente 8 dígitos numéricos")]
        public string Dni { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string Telefono { get; set; } = string.Empty;

        // <<< Asegúrate que la propiedad se llame exactamente Contrasena (C mayúscula)
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Contrasena { get; set; } = string.Empty;

        [Required]
        public string TipoPaciente { get; set; } = string.Empty;
    }
}

