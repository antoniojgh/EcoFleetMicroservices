namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;

/// <summary>
/// Published by DriverService when a driver is suspended via Driver.Suspend().
/// The domain guard prevents suspending a driver who is OnDuty — the driver
/// must be unassigned from a vehicle before suspension is possible.
///
/// This is the most critical driver event: it cascades to four services.
/// Consumed by:
///   - NotificationService: sends a suspension email to the driver
///   - FleetService: unassigns this driver from any vehicle (defensive sync)
///   - OrderService: auto-cancels any Pending orders assigned to this driver
///   - AssignmentService: deactivates active manager-driver assignments for this driver
/// </summary>
public record DriverSuspendedIntegrationEvent
{
    /// <summary>
    /// The unique identifier of the suspended driver (DriverId.Value).
    /// Primitive Guid — no strong typing across service boundaries.
    /// </summary>
    public Guid DriverId { get; init; }

    /// <summary>
    /// Driver's first name. Required by NotificationService to compose
    /// the suspension email body without a secondary HTTP call to DriverService.
    /// Matches the existing DriverSuspendedEventDTO pattern in the monolith.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Driver's last name. Required by NotificationService for email composition.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Driver's email address. The primary delivery target for NotificationService.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Driver's license number. Carried for audit trail and logging in FleetService.
    /// Sourced from DriverLicense.Value.
    /// </summary>
    public string License { get; init; } = string.Empty;

    /// <summary>
    /// UTC timestamp when this integration event was raised.
    /// </summary>
    public DateTime OccurredOn { get; init; }
}
