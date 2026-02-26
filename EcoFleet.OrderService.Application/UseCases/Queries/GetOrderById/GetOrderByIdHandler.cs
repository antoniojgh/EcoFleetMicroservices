using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.OrderService.Application.DTOs;
using EcoFleet.OrderService.Application.Interfaces;
using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Queries.GetOrderById;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderDetailDTO>
{
    private readonly IOrderReadRepository _readRepository;

    public GetOrderByIdHandler(IOrderReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<OrderDetailDTO> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _readRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Order", request.Id);

        return order;
    }
}
