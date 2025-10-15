using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Lab01_Grupo1.Models;
using System.Threading.Tasks;

public class PagoController : Controller
{
    private readonly ApplicationDbContext _context;

    public PagoController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Pago/ProcesarPago?medicoId=1&motivo=Consulta&fechaHora=...
    public async Task<IActionResult> ProcesarPago(int medicoId, string motivo, DateTime fechaHora)
    {
        var medico = await _context.Medicos
            .Include(m => m.Usuario)
            .FirstOrDefaultAsync(m => m.IdMedico == medicoId);

        if (medico == null) return NotFound();

        var model = new CitaPagoViewModel
        {
            MedicoId = medicoId,
            MedicoNombre = medico.Usuario.Nombre,
            Especialidad = medico.Especialidad,
            MotivoConsulta = motivo,
            FechaHora = fechaHora,
            Precio = 80.00m // Precio fijo por ahora
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarPago(CitaPagoViewModel model)
    {
        // ✅ 1. Aquí procesaríamos el pago con Stripe (por ahora simulamos)
        
        // ✅ 2. Si pago exitoso, crear la cita
        var cita = new Cita
        {
            IdPaciente = 1, // ⚠️ Obtener del usuario logueado
            IdMedico = model.MedicoId,
            Especialidad = _context.Medicos.First(m => m.IdMedico == model.MedicoId).Especialidad,
            FechaHora = model.FechaHora,
            MotivoConsulta = model.MotivoConsulta,
            Estado = "confirmada",
            CodigoCita = Guid.NewGuid().ToString().Substring(0, 8),
            // NUEVAS PROPIEDADES:
            Precio = model.Precio,
            EstadoPago = "pagado",
            FechaPago = DateTime.Now,
            MetodoPago = "stripe"
        };

        _context.Citas.Add(cita);
        await _context.SaveChangesAsync();

        TempData["Mensaje"] = "¡Cita agendada y pagada correctamente!";
        return RedirectToAction("Citas", "Usuarios");
    }
}