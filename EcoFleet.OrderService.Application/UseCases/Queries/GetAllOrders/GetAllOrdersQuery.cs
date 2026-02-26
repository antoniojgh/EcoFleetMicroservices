using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.OrderService.Application.DTOs;
using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Queries.GetAllOrders;

public record GetAllOrdersQuery : FilterOrderDTO, IRequest<PaginatedDTO<OrderDetailDTO>>;
