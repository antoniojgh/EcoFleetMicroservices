namespace EcoFleet.OrderService.API.Controllers;

/// <summary>
/// Request body for the cancel-order endpoint.
/// The order ID is taken from the URL; only the optional reason is in the body.
/// </summary>
public record CancelOrderRequest(string? CancellationReason = null);
