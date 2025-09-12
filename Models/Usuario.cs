using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Lab01_Grupo1.Models
{
    [Table("Usuario")]
    public class Usuario
    {
        [Key]
        [Column("id_usuario")]
        public int IdUsuario { get; set; }

        [Required]
        [StringLength(50)]
        [Column("usuario")]
        public string? UsuarioNombre { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Column("clave")]
        public string? Clave { get; set; }

        [Required]
        [StringLength(100)]
        [Column("nombre")]
        public string? Nombre { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        [Column("correo")]
        public string? Correo { get; set; }

        [Required]
        [RegularExpression("^(administrador|medico|usuario)$")]
        [Column("rol")]
        public string? Rol { get; set; }
    }
}
