using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.DriverService.Application.DTOs;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Entities;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Queries.GetDriverById;

public class GetDriverByIdHandler : IRequestHandler<GetDriverByIdQuery, DriverDetailDTO>
{
    private readonly IDriverRepository _repository;

    public GetDriverByIdHandler(IDriverRepository repository)
    {
        _repository = repository;
    }

    public async Task<DriverDetailDTO> Handle(GetDriverByIdQuery request, CancellationToken cancellationToken)
    {
        var driverId = new DriverId(request.Id);
        var driver = await _repository.GetByIdAsync(driverId, cancellationToken);

        if (driver is null)
        {
            throw new NotFoundException(nameof(Driver), request.Id);
        }

        return DriverDetailDTO.FromEntity(driver);
    }
}
