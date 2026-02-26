using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.ManagerService.Application.Interfaces;
using EcoFleet.ManagerService.Domain.Entities;
using MediatR;

namespace EcoFleet.ManagerService.Application.UseCases.Commands.DeleteManager;

public class DeleteManagerHandler : IRequestHandler<DeleteManagerCommand>
{
    private readonly IManagerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteManagerHandler(IManagerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteManagerCommand request, CancellationToken cancellationToken)
    {
        var managerId = new ManagerId(request.Id);
        var manager = await _repository.GetByIdAsync(managerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Manager), request.Id);

        await _repository.Delete(manager, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
