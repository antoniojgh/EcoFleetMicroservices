using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid DriverId,
    string DriverFirstName,
    string DriverLastName,
    string DriverEmail,
    double PickUpLatitude,
    double PickUpLongitude,
    double DropOffLatitude,
    double DropOffLongitude,
    decimal Price) : IRequest<Guid>;
