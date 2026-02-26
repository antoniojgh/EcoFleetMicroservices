using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.ManagerService.Application.DTOs;
using EcoFleet.ManagerService.Application.Interfaces;
using EcoFleet.ManagerService.Domain.Entities;
using MediatR;

namespace EcoFleet.ManagerService.Application.UseCases.Queries.GetManagerById;

public class GetManagerByIdHandler : IRequestHandler<GetManagerByIdQuery, ManagerDetailDTO>
{
    private readonly IManagerRepository _repository;

    public GetManagerByIdHandler(IManagerRepository repository)
    {
        _repository = repository;
    }

    public async Task<ManagerDetailDTO> Handle(GetManagerByIdQuery request, CancellationToken cancellationToken)
    {
        var managerId = new ManagerId(request.Id);
        var manager = await _repository.GetByIdAsync(managerId, cancellationToken);

        if (manager is null)
        {
            throw new NotFoundException(nameof(Manager), request.Id);
        }

        return ManagerDetailDTO.FromEntity(manager);
    }
}
