using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.ManagerService.Application.Interfaces;
using EcoFleet.ManagerService.Domain.Entities;
using EcoFleet.ManagerService.Domain.ValueObjects;
using MediatR;

namespace EcoFleet.ManagerService.Application.UseCases.Commands.CreateManager;

public class CreateManagerHandler : IRequestHandler<CreateManagerCommand, Guid>
{
    private readonly IManagerRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateManagerHandler(IManagerRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateManagerCommand request, CancellationToken cancellationToken)
    {
        var name = FullName.Create(request.FirstName, request.LastName);
        var email = Email.Create(request.Email);

        var manager = new Manager(name, email);

        await _repository.AddAsync(manager, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return manager.Id.Value;
    }
}
