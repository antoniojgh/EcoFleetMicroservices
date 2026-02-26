using EcoFleet.AssignmentService.Domain.Entities;
using EcoFleet.AssignmentService.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.AssignmentService.Infrastructure.Persistence;

public class AssignmentDbContext : DbContext
{
    public AssignmentDbContext(DbContextOptions<AssignmentDbContext> options) : base(options)
    {
    }

    public DbSet<ManagerDriverAssignment> Assignments { get; set; }
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AssignmentDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
