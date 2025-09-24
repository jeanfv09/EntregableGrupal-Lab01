using System.ComponentModel.DataAnnotations;

namespace Lab01_Grupo1.Models
{
    public class CitaViewModel
{
    public int IdMedico { get; set; }
    public string MotivoConsulta { get; set; } = string.Empty;
    public DateTime FechaHora { get; set; }

    // Opcional: para mostrar en el formulario
    public List<Medico> Medicos { get; set; } = new();
}

}
