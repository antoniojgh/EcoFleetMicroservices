using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.OrderService.Application.DTOs;
using EcoFleet.OrderService.Application.Interfaces;
using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Queries.GetAllOrders;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, PaginatedDTO<OrderDetailDTO>>
{
    private readonly IOrderReadRepository _readRepository;

    public GetAllOrdersHandler(IOrderReadRepository readRepository)
    {
        _readRepository = readRepository;
    }

    public async Task<PaginatedDTO<OrderDetailDTO>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        return await _readRepository.GetFilteredAsync(request, cancellationToken);
    }
}
