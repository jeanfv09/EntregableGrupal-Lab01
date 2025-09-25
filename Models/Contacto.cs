using System.ComponentModel.DataAnnotations;

namespace Lab01_Grupo1.Models
{
    public class Contacto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
}


}
