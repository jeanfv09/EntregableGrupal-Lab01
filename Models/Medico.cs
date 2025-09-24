using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

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

        [Required]
        [Column("especialidad")]
        public string Especialidad { get; set; } = null!;

        [ForeignKey("IdUsuario")]
        public Usuario Usuario { get; set; } = null!;

        public PerfilMedico? Perfil { get; set; }
        public ICollection<TelefonoMedico> Telefonos { get; set; } = new List<TelefonoMedico>();
    }
}
