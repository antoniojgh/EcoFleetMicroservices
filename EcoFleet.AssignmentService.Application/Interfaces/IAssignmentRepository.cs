using EcoFleet.AssignmentService.Application.DTOs;
using EcoFleet.AssignmentService.Domain.Entities;
using EcoFleet.BuildingBlocks.Application.Interfaces;

namespace EcoFleet.AssignmentService.Application.Interfaces;

public interface IAssignmentRepository : IRepository<ManagerDriverAssignment, ManagerDriverAssignmentId>
{
    /// <summary>
    /// Returns all active assignments for the specified driver.
    /// Used by DriverSuspendedConsumer to deactivate assignments when a driver is suspended.
    /// </summary>
    Task<IEnumerable<ManagerDriverAssignment>> GetActiveByDriverIdAsync(Guid driverId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a filtered and paginated list of assignments.
    /// </summary>
    Task<IEnumerable<ManagerDriverAssignment>> GetFilteredAsync(FilterAssignmentDTO filter, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the total number of assignments matching the filter criteria (before pagination).
    /// Used alongside GetFilteredAsync to populate PaginatedDTO.TotalCount correctly.
    /// </summary>
    Task<int> GetFilteredCountAsync(FilterAssignmentDTO filter, CancellationToken cancellationToken = default);
}
