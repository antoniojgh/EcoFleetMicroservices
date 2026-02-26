using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Domain.Exceptions;

namespace EcoFleet.AssignmentService.Domain.Entities;

public class ManagerDriverAssignment : Entity<ManagerDriverAssignmentId>, IAggregateRoot
{
    /// <summary>
    /// Primitive Guid — no cross-boundary strong typing.
    /// References the Manager in ManagerService.
    /// </summary>
    public Guid ManagerId { get; private set; }

    /// <summary>
    /// Primitive Guid — no cross-boundary strong typing.
    /// References the Driver in DriverService.
    /// </summary>
    public Guid DriverId { get; private set; }

    public bool IsActive { get; private set; }
    public DateTime AssignedDate { get; private set; }

    // Constructor for EF Core
    private ManagerDriverAssignment() : base(new ManagerDriverAssignmentId(Guid.NewGuid()))
    { }

    public ManagerDriverAssignment(Guid managerId, Guid driverId)
        : base(new ManagerDriverAssignmentId(Guid.NewGuid()))
    {
        ManagerId = managerId;
        DriverId = driverId;
        IsActive = true;
        AssignedDate = DateTime.UtcNow;
    }

    // --- BEHAVIORS ---

    public void Deactivate()
    {
        if (!IsActive)
            throw new DomainException("Assignment is already deactivated.");

        IsActive = false;
    }
}
