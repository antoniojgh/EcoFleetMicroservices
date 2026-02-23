using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.DriverService.Application.DTOs;
using EcoFleet.DriverService.Application.Interfaces;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Queries.GetAllDrivers;

public class GetAllDriversHandler : IRequestHandler<GetAllDriversQuery, PaginatedDTO<DriverDetailDTO>>
{
    private readonly IDriverRepository _repository;

    public GetAllDriversHandler(IDriverRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedDTO<DriverDetailDTO>> Handle(GetAllDriversQuery request, CancellationToken cancellationToken)
    {
        var driversFiltered = await _repository.GetFilteredAsync(request, cancellationToken);

        var driversFilteredDTO = driversFiltered.Select(DriverDetailDTO.FromEntity);

        var paginatedResult = new PaginatedDTO<DriverDetailDTO>
        {
            Items = driversFilteredDTO,
            TotalCount = driversFilteredDTO.Count()
        };

        return paginatedResult;
    }
}
