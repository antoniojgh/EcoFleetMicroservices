using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.OrderService.Application.DTOs;

namespace EcoFleet.OrderService.Application.Interfaces;

public interface IOrderReadRepository
{
    Task<OrderDetailDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PaginatedDTO<OrderDetailDTO>> GetFilteredAsync(FilterOrderDTO filter, CancellationToken cancellationToken = default);
}
