using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.OrderService.Application.DTOs;
using EcoFleet.OrderService.Application.Interfaces;
using EcoFleet.OrderService.Infrastructure.Projections;
using Marten;

namespace EcoFleet.OrderService.Infrastructure.Repositories;

/// <summary>
/// Queries the Marten read model (OrderReadModel) projected by OrderReadModelProjection.
/// This replaces EF Core-based repositories since OrderService is purely event-sourced.
/// </summary>
public class OrderReadRepository : IOrderReadRepository
{
    private readonly IQuerySession _querySession;

    public OrderReadRepository(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<OrderDetailDTO?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var model = await _querySession.LoadAsync<OrderReadModel>(id, cancellationToken);

        return model is null ? null : MapToDto(model);
    }

    public async Task<PaginatedDTO<OrderDetailDTO>> GetFilteredAsync(FilterOrderDTO filter, CancellationToken cancellationToken = default)
    {
        var query = _querySession.Query<OrderReadModel>().AsQueryable();

        if (filter.Id.HasValue)
            query = query.Where(x => x.Id == filter.Id.Value);

        if (filter.DriverId.HasValue)
            query = query.Where(x => x.DriverId == filter.DriverId.Value);

        if (!string.IsNullOrEmpty(filter.Status))
            query = query.Where(x => x.Status == filter.Status);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((filter.Page - 1) * filter.RecordsByPage)
            .Take(filter.RecordsByPage)
            .ToListAsync(cancellationToken);

        return new PaginatedDTO<OrderDetailDTO>
        {
            TotalCount = totalCount,
            Items = items.Select(MapToDto)
        };
    }

    private static OrderDetailDTO MapToDto(OrderReadModel model) => new(
        model.Id,
        model.DriverId,
        model.Status,
        model.PickUpLatitude,
        model.PickUpLongitude,
        model.DropOffLatitude,
        model.DropOffLongitude,
        model.Price,
        model.CancellationReason,
        model.CreatedDate);
}
