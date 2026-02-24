using EcoFleet.FleetService.Domain.Entities;
using EcoFleet.FleetService.Domain.ValueObjects;
using EcoFleet.FleetService.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EcoFleet.FleetService.Infrastructure.Persistence.Configurations
{
    public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
    {
        public void Configure(EntityTypeBuilder<OutboxMessage> builder)
        {
            builder.ToTable("OutboxMessages");
            builder.HasKey(o => o.Id);
            builder.Property(o => o.Type).HasMaxLength(500).IsRequired();
            builder.Property(o => o.Content).IsRequired();
            builder.Property(o => o.OccurredOn).IsRequired();
            builder.Property(o => o.ProcessedOn).IsRequired(false);
            builder.Property(o => o.Error).IsRequired(false);
        }
    }
}
