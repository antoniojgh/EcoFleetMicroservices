using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.FleetService.Domain.Entities;

namespace EcoFleet.FleetService.Domain.Events;

public record VehicleDriverAssignedEvent(
    VehicleId VehicleId,
    Guid DriverId,
    DateTime OcurredOn
) : IDomainEvent;
