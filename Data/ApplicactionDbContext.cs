using Microsoft.EntityFrameworkCore;
namespace Lab01_Grupo1.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }


}