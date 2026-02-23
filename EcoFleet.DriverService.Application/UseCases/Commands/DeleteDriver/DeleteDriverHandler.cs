using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.DriverService.Application.Interfaces;
using EcoFleet.DriverService.Domain.Entities;
using EcoFleet.DriverService.Domain.Enums;
using MediatR;

namespace EcoFleet.DriverService.Application.UseCases.Commands.DeleteDriver;

public class DeleteDriverHandler : IRequestHandler<DeleteDriverCommand>
{
    private readonly IDriverRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteDriverHandler(IDriverRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteDriverCommand request, CancellationToken cancellationToken)
    {
        var driverId = new DriverId(request.Id);
        var driver = await _repository.GetByIdAsync(driverId, cancellationToken);

        if (driver is null)
        {
            throw new NotFoundException(nameof(Driver), request.Id);
        }

        if (driver.Status == DriverStatus.OnDuty)
        {
            throw new BusinessRuleException("Cannot delete a driver who is currently on duty. Unassign them first.");
        }

        await _repository.Delete(driver, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
