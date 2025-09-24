using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lab01_Grupo1.Models
{
    public class MedicoCreateViewModel
    {
        // Usuario
       [Required]
        public string UsuarioNombre { get; set; } = null!;
        [Required]
        public string Clave { get; set; } = null!;
        [Required]
        public string Nombre { get; set; } = null!;
        [Required, EmailAddress]
        public string Correo { get; set; } = null!;

        // Medico
        [Required]
        public string Especialidad { get; set; } = null!;

        // Perfil
        public string Universidad { get; set; } = null!;
        public string PaisFormacion { get; set; } = null!;
        public string Egreso { get; set; } = null!;
        public string Experiencia { get; set; } = null!;
        public string Idiomas { get; set; } = null!;

        [Required]
        [RegularExpression("planta|temporal|externo", 
            ErrorMessage = "El tipo de contrato debe ser planta, temporal o externo.")]
        public string TipoContrato { get; set; } = null!;

        [Required]
        [RegularExpression("mañana|tarde|noche", 
            ErrorMessage = "El turno preferido debe ser tarde o noche.")]
        public string TurnoPreferido { get; set; } = null!;

        public List<string> Telefonos { get; set; } = new();
    }
}
