namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;

/// <summary>
/// Published by DriverService when a suspended driver is reinstated via Driver.Reinstate().
/// The domain guard ensures only Suspended drivers can be reinstated.
///
/// Consumed by:
///   - NotificationService: sends a reinstatement confirmation email to the driver
///   - AssignmentService: may reactivate previously deactivated assignments (optional)
/// </summary>
public record DriverReinstatedIntegrationEvent
{
    /// <summary>
    /// The unique identifier of the reinstated driver (DriverId.Value).
    /// Primitive Guid — no strong typing across service boundaries.
    /// </summary>
    public Guid DriverId { get; init; }

    /// <summary>
    /// Driver's first name. Required by NotificationService to compose
    /// the reinstatement email without a secondary HTTP call to DriverService.
    /// Matches the existing DriverReinstatedEventDTO pattern in the monolith.
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
    /// UTC timestamp when this integration event was raised.
    /// </summary>
    public DateTime OccurredOn { get; init; }
}
