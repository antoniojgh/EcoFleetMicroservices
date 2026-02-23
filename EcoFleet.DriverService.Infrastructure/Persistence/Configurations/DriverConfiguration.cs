using EcoFleet.DriverService.Domain.Entities;
using EcoFleet.DriverService.Domain.ValueObjects;
using EcoFleet.DriverService.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFleet.DriverService.Infrastructure.Persistence.Configurations;

public class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("Drivers");

        // 1. Primary Key (Strongly Typed ID)
        builder.HasKey(d => d.Id);

        // Convert DriverId record <-> Guid for Database
        builder.Property(d => d.Id)
            .HasConversion(
                id => id.Value,
                value => new DriverId(value));

        // 2. Value Object: FullName (Multiple Columns via OwnsOne)
        builder.OwnsOne(d => d.Name, nameBuilder =>
        {
            nameBuilder.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(100)
                .IsRequired();

            nameBuilder.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(100)
                .IsRequired();
        });

        // 3. Value Object: DriverLicense (Single Column)
        builder.Property(d => d.License)
            .HasConversion(
                license => license.Value,
                value => DriverLicense.Create(value))
            .HasMaxLength(20)
            .IsRequired();

        // 4. Value Object: Email (Single Column)
        builder.Property(d => d.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasMaxLength(256)
            .IsRequired();

        // 5. Value Object: PhoneNumber (Single Column, Nullable)
        builder.Property(d => d.PhoneNumber)
            .HasConversion(
                phone => phone != null ? phone.Value : null,
                value => value != null ? PhoneNumber.Create(value) : null)
            .HasMaxLength(20)
            .IsRequired(false);

        // 6. DateOfBirth (Nullable)
        builder.Property(d => d.DateOfBirth)
            .IsRequired(false);

        // 7. Enum as String
        builder.Property(d => d.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // 8. AssignedVehicleId — stored as primitive Guid (no cross-service strong typing)
        builder.Property(d => d.AssignedVehicleId)
            .IsRequired(false);
    }
}
