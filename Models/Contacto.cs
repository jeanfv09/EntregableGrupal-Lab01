using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab01_Grupo1.Models
{
    public class Contacto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public DateTime FechaEnviado { get; set; } = DateTime.Now;

[Column("EtiquetaPredicha")]
        public string? Sentimiento { get; set; }  // ahora nullable

        [Column("ScorePrediccion")]
        public float? ScorePrediccion { get; set; } // nullable numérico
    }
}
