using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab01_Grupo1.Models
{
    [Table("Cita")]
    public class Cita
    {
        [Key]
        [Column("id_cita")]
        public int IdCita { get; set; }

        [Required]
        [Column("id_paciente")]
        public int IdPaciente { get; set; }

        [Required]
        [Column("especialidad")]
        public string Especialidad { get; set; } = string.Empty;

        [Column("id_medico")]
        public int? IdMedico { get; set; }

        [Column("asignado_por")]
        public int? AsignadoPor { get; set; }

        [Required]
        [Column("fecha_hora")]
        public DateTime FechaHora { get; set; }

        [Column("estado")]
        public string Estado { get; set; } = "pendiente";

        [Column("motivo_consulta")]
        public string? MotivoConsulta { get; set; }

        [Column("prioridad")]
        public string? Prioridad { get; set; } // "alta", "media", "baja"

        [Column("observaciones")]
        public string? Observaciones { get; set; }

        [Column("codigo_cita")]
        public string? CodigoCita { get; set; }

        // --- Campos de Pago ---

        [Column("precio")]
        public decimal Precio { get; set; } = 80.00m;

        [Column("estado_pago")]
        public string EstadoPago { get; set; } = "pendiente";

        [Column("fecha_pago")]
        public DateTime? FechaPago { get; set; }

        [Column("transaction_id")]
        public string? TransactionId { get; set; }

        [Column("metodo_pago")]
        public string? MetodoPago { get; set; }

        // API PAYPAL

        [Column("precio")]
        public decimal Precio { get; set; } = 80.00m;

        [Column("estado_pago")]
        public string EstadoPago { get; set; } = "pendiente";

        [Column("fecha_pago")]
        public DateTime? FechaPago { get; set; }

        [Column("transaction_id")]
        public string? TransactionId { get; set; }

        [Column("metodo_pago")]
        public string? MetodoPago { get; set; }



        // 🔗 Relaciones
        [ForeignKey("IdPaciente")]
        public Usuario Paciente { get; set; } = null!;

        [ForeignKey("IdMedico")]
        public Medico? Medico { get; set; }

        [ForeignKey("AsignadoPor")]
        public Usuario? Asignador { get; set; }
    }
}
