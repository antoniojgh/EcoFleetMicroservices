using System.Reflection.Emit;
using EcoFleet.FleetService.Domain.Entities;
using EcoFleet.FleetService.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.FleetService.Infrastructure.Persistence;

public class FleetDbContext : DbContext
{
    public FleetDbContext(DbContextOptions<FleetDbContext> options) : base(options)
    {
    }

    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FleetDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
