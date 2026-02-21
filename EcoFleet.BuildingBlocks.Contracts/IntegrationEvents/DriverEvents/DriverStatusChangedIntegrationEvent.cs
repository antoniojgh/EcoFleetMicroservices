namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;

/// <summary>
/// Published by DriverService whenever a driver's status transitions to any new value.
/// This is the generic status-change event, published in addition to the specific
/// events (DriverSuspendedIntegrationEvent, DriverReinstatedIntegrationEvent).
///
/// Use this event for services that need to react to ANY status transition without
/// subscribing to every specific event — for example, a real-time dashboard service
/// or an audit log service.
///
/// Consumed by:
///   - Any service tracking driver availability in aggregate (dashboards, audit logs)
/// </summary>
public record DriverStatusChangedIntegrationEvent
{
    /// <summary>
    /// The unique identifier of the driver whose status changed (DriverId.Value).
    /// Primitive Guid — no strong typing across service boundaries.
    /// </summary>
    public Guid DriverId { get; init; }

    /// <summary>
    /// Driver's first name, included for display and logging by consumers.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Driver's last name, included for display and logging by consumers.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// The driver's status before this transition.
    /// Serialized as string to avoid enum coupling across service boundaries.
    /// Valid values: "Available" | "OnDuty" | "Suspended"
    /// </summary>
    public string PreviousStatus { get; init; } = string.Empty;

    /// <summary>
    /// The driver's status after this transition.
    /// Serialized as string to avoid enum coupling across service boundaries.
    /// Valid values: "Available" | "OnDuty" | "Suspended"
    /// </summary>
    public string NewStatus { get; init; } = string.Empty;

    /// <summary>
    /// UTC timestamp when this integration event was raised.
    /// </summary>
    public DateTime OccurredOn { get; init; }
}
