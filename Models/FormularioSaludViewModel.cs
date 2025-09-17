using System.ComponentModel.DataAnnotations;

namespace Lab01_Grupo1.Models
{
    public class FormularioSaludViewModel
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "La edad es obligatoria")]
        [Range(0, 120, ErrorMessage = "Ingrese una edad válida")]
        public int Edad { get; set; }

        [Required(ErrorMessage = "El peso es obligatorio")]
        [Range(1, 500, ErrorMessage = "Ingrese un peso válido")]
        public double Peso { get; set; }

        [Required(ErrorMessage = "La altura es obligatoria")]
        [Range(30, 250, ErrorMessage = "Ingrese una altura válida en cm")]
        public double Altura { get; set; }

        [Required(ErrorMessage = "Debe ingresar los síntomas")]
        public string Sintomas { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe ingresar antecedentes médicos")]
        public string Antecedentes { get; set; } = string.Empty;
    }
}

