using System;

namespace Lab01_Grupo1.Models 
{
    public class CitaPagoViewModel
    {
        public int MedicoId { get; set; }
        public string MedicoNombre { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string MotivoConsulta { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public decimal Precio { get; set; }
    }
}