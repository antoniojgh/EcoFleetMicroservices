using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.FleetService.Domain.Entities;

namespace EcoFleet.FleetService.Domain.Events;

public record VehicleDriverUnassignedEvent(
    VehicleId VehicleId,
    DateTime OcurredOn
) : IDomainEvent;
