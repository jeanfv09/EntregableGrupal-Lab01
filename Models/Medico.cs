using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab01_Grupo1.Models
{
    [Table("Medico")]
    public class Medico
    {
        [Key]
        [Column("id_medico")]
        public int IdMedico { get; set; }

        [Required]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        // Relación 1:1 con Usuario
        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;
    }
}
