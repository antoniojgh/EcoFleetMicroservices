namespace EcoFleet.BuildingBlocks.Contracts.IntegrationEvents.DriverEvents;

/// <summary>
/// Published by DriverService when a new driver is successfully created.
/// Consumed by:
///   - AssignmentService: to know the driver exists before creating assignments
///   - NotificationService: to send a welcome email (optional)
/// </summary>
public record DriverCreatedIntegrationEvent
{
    /// <summary>
    /// The unique identifier of the newly created driver (DriverId.Value).
    /// Primitive Guid — no strong typing across service boundaries.
    /// </summary>
    public Guid DriverId { get; init; }

    /// <summary>
    /// Driver's first name, sourced from FullName.FirstName value object.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Driver's last name, sourced from FullName.LastName value object.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Driver's license number, sourced from DriverLicense.Value.
    /// Stored uppercase with a maximum of 20 characters in the domain.
    /// </summary>
    public string License { get; init; } = string.Empty;

    /// <summary>
    /// Driver's email address, sourced from Email.Value.
    /// Stored lowercase with validated format in the domain.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Driver's phone number. Nullable because it is optional in the domain
    /// (Driver constructor accepts PhoneNumber? phoneNumber = null).
    /// </summary>
    public string? PhoneNumber { get; init; }

    /// <summary>
    /// Driver's date of birth. Nullable because it is optional in the domain.
    /// </summary>
    public DateTime? DateOfBirth { get; init; }

    /// <summary>
    /// Driver status at creation time. Always "Available" on creation.
    /// Serialized as string to avoid enum coupling across service boundaries.
    /// Valid values: "Available" | "OnDuty" | "Suspended"
    /// </summary>
    public string Status { get; init; } = "Available";

    /// <summary>
    /// UTC timestamp when this integration event was raised.
    /// Note: the domain uses OcurredOn (typo) — this library uses the correct spelling.
    /// </summary>
    public DateTime OccurredOn { get; init; }
}
