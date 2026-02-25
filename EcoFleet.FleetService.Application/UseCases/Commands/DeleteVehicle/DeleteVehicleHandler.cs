using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Application.Interfaces;
using EcoFleet.FleetService.Application.Interfaces;
using EcoFleet.FleetService.Domain.Entities;
using EcoFleet.FleetService.Domain.Enums;
using MediatR;

namespace EcoFleet.FleetService.Application.UseCases.Commands.DeleteVehicle;

public class DeleteVehicleHandler : IRequestHandler<DeleteVehicleCommand>
{
    private readonly IVehicleRepository _vehicleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVehicleHandler(IVehicleRepository vehicleRepository, IUnitOfWork unitOfWork)
    {
        _vehicleRepository = vehicleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicleId = new VehicleId(request.Id);
        var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Vehicle), request.Id);

        if (vehicle.Status == VehicleStatus.Active)
            throw new BusinessRuleException("Cannot delete a vehicle that is currently active. Unassign the driver first.");

        await _vehicleRepository.Delete(vehicle, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
