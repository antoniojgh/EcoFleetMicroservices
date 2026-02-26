using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.ManagerService.Application.Interfaces;
using EcoFleet.ManagerService.Domain.Entities;
using EcoFleet.ManagerService.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.ManagerService.Application.UseCases.Commands.UpdateManager;

public class UpdateManagerHandler : IRequestHandler<UpdateManagerCommand>
{
    private readonly IManagerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateManagerHandler(IManagerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(UpdateManagerCommand request, CancellationToken cancellationToken)
    {
        var managerId = new ManagerId(request.Id);
        var manager = await _repository.GetByIdAsync(managerId, cancellationToken)
            ?? throw new NotFoundException(nameof(Manager), request.Id);

        var name = FullName.Create(request.FirstName, request.LastName);
        var email = Email.Create(request.Email);

        manager.UpdateName(name);
        manager.UpdateEmail(email);

        await _repository.Update(manager, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
