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
        var managersFiltered = await _repository.GetFilteredAsync(request, cancellationToken);

        var managersFilteredDTO = managersFiltered.Select(ManagerDetailDTO.FromEntity);

        var paginatedResult = new PaginatedDTO<ManagerDetailDTO>
        {
            Items = managersFilteredDTO,
            TotalCount = managersFilteredDTO.Count()
        };

        return paginatedResult;
    }
}
