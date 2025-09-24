using Microsoft.EntityFrameworkCore;
namespace Lab01_Grupo1.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Medico> Medicos { get; set; }
    public DbSet<PerfilMedico> PerfilesMedico { get; set; }
    public DbSet<TelefonoMedico> TelefonosMedico { get; set; }
    public DbSet<Cita> Citas { get; set; }
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Relación Medico - PerfilMedico (1:1)
    modelBuilder.Entity<Medico>()
        .HasOne(m => m.Perfil)
        .WithOne()
        .HasForeignKey<PerfilMedico>(p => p.IdMedico);

    // Relación Medico - Telefonos (1:N)
    modelBuilder.Entity<TelefonoMedico>()
        .HasOne(t => t.Medico)
        .WithMany(m => m.Telefonos)
        .HasForeignKey(t => t.IdMedico)
        .HasConstraintName("FK_TelefonoMedico_Medico");
}



}