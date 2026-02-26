using EcoFleet.OrderService.Application.DTOs;
using MediatR;

namespace EcoFleet.OrderService.Application.UseCases.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderDetailDTO>;
