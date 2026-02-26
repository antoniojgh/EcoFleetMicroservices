using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Commands.CreateOrder;

public record CreateOrderCommand(
    Guid DriverId,
    double PickUpLatitude,
    double PickUpLongitude,
    double DropOffLatitude,
    double DropOffLongitude,
    decimal Price) : IRequest<Guid>;
