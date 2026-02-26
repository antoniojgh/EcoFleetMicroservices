using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.ManagerService.Application.DTOs;
using EcoFleet.ManagerService.Application.Interfaces;
using MediatR;

namespace EcoFleet.ManagerService.Application.UseCases.Queries.GetAllManagers;

public class GetAllManagersHandler : IRequestHandler<GetAllManagersQuery, PaginatedDTO<ManagerDetailDTO>>
{
    private readonly IManagerRepository _repository;

    public GetAllManagersHandler(IManagerRepository repository)
    {
        _repository = repository;
    }

    public async Task<PaginatedDTO<ManagerDetailDTO>> Handle(GetAllManagersQuery request, CancellationToken cancellationToken)
    {
        var totalCount = await _repository.GetFilteredCountAsync(request, cancellationToken);
        var managersFiltered = await _repository.GetFilteredAsync(request, cancellationToken);

        return new PaginatedDTO<ManagerDetailDTO>
        {
            Items = managersFiltered.Select(ManagerDetailDTO.FromEntity),
            TotalCount = totalCount
        };
    }
}
