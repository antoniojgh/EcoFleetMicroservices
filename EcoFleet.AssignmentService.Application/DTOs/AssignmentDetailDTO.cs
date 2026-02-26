using EcoFleet.AssignmentService.Domain.Entities;

namespace EcoFleet.AssignmentService.Application.DTOs;

public record AssignmentDetailDTO(
    Guid Id,
    Guid ManagerId,
    Guid DriverId,
    bool IsActive,
    DateTime AssignedDate
)
{
    public static AssignmentDetailDTO FromEntity(ManagerDriverAssignment assignment) =>
        new(
            assignment.Id.Value,
            assignment.ManagerId,
            assignment.DriverId,
            assignment.IsActive,
            assignment.AssignedDate
        );
}
