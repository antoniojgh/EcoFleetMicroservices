using EcoFleet.DriverService.Application.DTOs;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Queries.GetDriverById;

public record GetDriverByIdQuery(Guid Id) : IRequest<DriverDetailDTO>;
