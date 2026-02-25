using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.FleetService.Application.DTOs;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Entities;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Queries.GetVehicleById;

public class GetVehicleByIdHandler : IRequestHandler<GetVehicleByIdQuery, VehicleDetailDTO>
{
    private readonly IVehicleRepository _repository;

    public GetVehicleByIdHandler(IVehicleRepository repository)
    {
        _repository = repository;
    }

    public async Task<VehicleDetailDTO> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
    {
        var vehicleId = new VehicleId(request.Id);
        var vehicle = await _repository.GetByIdAsync(vehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.Id);

        return VehicleDetailDTO.FromEntity(vehicle);
    }
}
