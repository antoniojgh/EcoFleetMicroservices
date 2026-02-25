using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.FleetService.Application.DTOs;
using EcoFleet.FleetService.Application.Interfaces;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Queries.GetAllVehicles;

public class GetAllVehiclesHandler : IRequestHandler<GetAllVehiclesQuery, PaginatedDTO<VehicleDetailDTO>>
{
    private readonly IVehicleRepository _repository;

    public GetAllVehiclesHandler(IVehicleRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedDTO<VehicleDetailDTO>> Handle(GetAllVehiclesQuery request, CancellationToken cancellationToken)
    {
        var vehiclesFiltered = await _repository.GetFilteredAsync(request, cancellationToken);

        var vehiclesFilteredDTO = vehiclesFiltered.Select(VehicleDetailDTO.FromEntity);

        var paginatedResult = new PaginatedDTO<VehicleDetailDTO>
        {
            Items = vehiclesFilteredDTO,
            TotalCount = vehiclesFilteredDTO.Count()
        };

        return paginatedResult;
    }
}
