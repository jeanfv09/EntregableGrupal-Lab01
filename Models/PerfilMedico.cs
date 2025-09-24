using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab01_Grupo1.Models
{
    [Table("Perfil_Medico")]
    public class PerfilMedico
    {
        [Key]
        [Column("id_perfil")]
        public int IdPerfil { get; set; }

        [Required]
        [Column("id_medico")]
        public int IdMedico { get; set; }

        [Column("universidad")]
        public string Universidad { get; set; } = null!;

        [Column("pais_formacion")]
        public string PaisFormacion { get; set; } = null!;

        [Column("egreso")]
        public string Egreso { get; set; } = null!;

        [Column("experiencia")]
        public string Experiencia { get; set; } = null!;

        [Column("idiomas")]
        public string Idiomas { get; set; } = null!;

        [Column("tipo_contrato")]
        public string TipoContrato { get; set; } = null!;

        [Column("turno_preferido")]
        public string TurnoPreferido { get; set; } = null!;
    }
}
