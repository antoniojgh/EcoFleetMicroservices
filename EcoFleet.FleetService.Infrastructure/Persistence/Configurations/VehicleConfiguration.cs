using EcoFleet.FleetService.Domain.Entities;
using EcoFleet.FleetService.Domain.ValueObjects;
using EcoFleet.FleetService.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFleet.FleetService.Infrastructure.Persistence.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        // 1. Primary Key (Strongly Typed ID)
        builder.HasKey(v => v.Id);

        // Convert VehicleId record <-> Guid for Database
        builder.Property(v => v.Id)
            .HasConversion(
                id => id.Value,
                value => new VehicleId(value));

        // 2. Value Object: LicensePlate (Single Column)
        builder.Property(v => v.Plate)
            .HasConversion(
                plate => plate.Value,
                value => LicensePlate.Create(value))
            .HasMaxLength(20)
            .IsRequired();

        // 3. Value Object: Geolocation (Multiple Columns via OwnsOne)
        builder.OwnsOne(v => v.CurrentLocation, locationBuilder =>
        {
            locationBuilder.Property(g => g.Latitude)
                .HasColumnName("Latitude")
                .IsRequired();

            locationBuilder.Property(g => g.Longitude)
                .HasColumnName("Longitude")
                .IsRequired();
        });

        // 4. Enum as String
        builder.Property(v => v.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // 5. CurrentDriverId — stored as primitive Guid (no cross-service strong typing)
        builder.Property(v => v.CurrentDriverId)
            .IsRequired(false);
    }
}


