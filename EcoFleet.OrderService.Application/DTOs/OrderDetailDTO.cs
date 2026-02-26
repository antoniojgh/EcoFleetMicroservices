namespace EcoFleet.OrderService.Application.DTOs;

public record OrderDetailDTO(
    Guid Id,
    Guid DriverId,
    string Status,
    double PickUpLatitude,
    double PickUpLongitude,
    double DropOffLatitude,
    double DropOffLongitude,
    decimal Price,
    string? CancellationReason,
    DateTime CreatedDate);
