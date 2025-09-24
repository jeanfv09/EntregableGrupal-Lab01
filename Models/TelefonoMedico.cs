using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab01_Grupo1.Models
{
    [Table("Telefono_Medico")]
    public class TelefonoMedico
    {
        [Key]
        [Column("id_telefono")]
        public int IdTelefono { get; set; }

        [Required]
        [Column("id_medico")]
        public int IdMedico { get; set; }

        [Column("telefono")]
        public string Telefono { get; set; } = null!;

        [ForeignKey("IdMedico")]
        public Medico Medico { get; set; } = null!;
    }
}
