using EcoFleet.AssignmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFleet.AssignmentService.Infrastructure.Persistence.Configurations;

public class ManagerDriverAssignmentConfiguration : IEntityTypeConfiguration<ManagerDriverAssignment>
{
    public void Configure(EntityTypeBuilder<ManagerDriverAssignment> builder)
    {
        builder.ToTable("ManagerDriverAssignments");

        // 1. Primary Key (Strongly Typed ID)
        builder.HasKey(a => a.Id);

        // Convert ManagerDriverAssignmentId record <-> Guid for Database
        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => new ManagerDriverAssignmentId(value));

        // 2. ManagerId (primitive Guid — no cross-boundary strong typing)
        builder.Property(a => a.ManagerId)
            .IsRequired();

        // 3. DriverId (primitive Guid — no cross-boundary strong typing)
        builder.Property(a => a.DriverId)
            .IsRequired();

        // 4. IsActive
        builder.Property(a => a.IsActive)
            .IsRequired();

        // 5. AssignedDate
        builder.Property(a => a.AssignedDate)
            .IsRequired();

        // 6. Index for fast lookup by DriverId (used by DriverSuspendedConsumer)
        builder.HasIndex(a => a.DriverId);

        // 7. Index for fast lookup by ManagerId
        builder.HasIndex(a => a.ManagerId);
    }
}
