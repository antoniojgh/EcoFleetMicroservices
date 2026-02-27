using EcoFleet.DriverService.Domain.Entities;
using EcoFleet.DriverService.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.DriverService.Infrastructure.Persistence;

public class DriverDbContext : DbContext
{
    public DriverDbContext(DbContextOptions<DriverDbContext> options) : base(options)
    {
    }

    public DbSet<Driver> Drivers { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DriverDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
