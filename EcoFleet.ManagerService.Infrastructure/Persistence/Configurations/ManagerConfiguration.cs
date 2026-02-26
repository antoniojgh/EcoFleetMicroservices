using EcoFleet.ManagerService.Domain.Entities;
using EcoFleet.ManagerService.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFleet.ManagerService.Infrastructure.Persistence.Configurations;

public class ManagerConfiguration : IEntityTypeConfiguration<Manager>
{
    public void Configure(EntityTypeBuilder<Manager> builder)
    {
        builder.ToTable("Managers");

        // 1. Primary Key (Strongly Typed ID)
        builder.HasKey(m => m.Id);

        // Convert ManagerId record <-> Guid for Database
        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                value => new ManagerId(value));

        // 2. Value Object: FullName (Multiple Columns via OwnsOne)
        builder.OwnsOne(m => m.Name, nameBuilder =>
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

        // 3. Value Object: Email (Single Column)
        builder.Property(m => m.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value))
            .HasMaxLength(256)
            .IsRequired();
    }
}
