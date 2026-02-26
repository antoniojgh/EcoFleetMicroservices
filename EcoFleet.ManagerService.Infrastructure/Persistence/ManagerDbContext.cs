using EcoFleet.ManagerService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.ManagerService.Infrastructure.Persistence;

public class ManagerDbContext : DbContext
{
    public ManagerDbContext(DbContextOptions<ManagerDbContext> options) : base(options)
    {
    }

    public DbSet<Manager> Managers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ManagerDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
