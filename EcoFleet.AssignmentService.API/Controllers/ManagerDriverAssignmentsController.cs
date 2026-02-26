using EcoFleet.AssignmentService.Application.DTOs;
using EcoFleet.AssignmentService.Application.UseCases.Commands.CreateAssignment;
using EcoFleet.AssignmentService.Application.UseCases.Commands.DeactivateAssignment;
using EcoFleet.AssignmentService.Application.UseCases.Commands.DeleteAssignment;
using EcoFleet.AssignmentService.Application.UseCases.Queries.GetAllAssignments;
using EcoFleet.AssignmentService.Application.UseCases.Queries.GetAssignmentById;
using EcoFleet.BuildingBlocks.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoFleet.AssignmentService.API.Controllers;

/// <summary>
/// Manages manager-driver assignments — create, query, deactivate, and delete.
/// ManagerId and DriverId are primitive Guids — no cross-boundary strong typing.
/// Manager/driver validation uses eventual consistency.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
public class ManagerDriverAssignmentsController : ControllerBase
{
    private readonly ISender _sender;

    public ManagerDriverAssignmentsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves a paginated list of assignments with optional filters.
    /// </summary>
    /// <param name="query">Optional filters: managerId, driverId, isActive, and pagination parameters.</param>
    /// <returns>A paginated list of assignments matching the specified filters.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedDTO<AssignmentDetailDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllAssignments([FromQuery] GetAllAssignmentsQuery query)
    {
        var result = await _sender.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single assignment by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the assignment.</param>
    /// <returns>The assignment details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AssignmentDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssignmentById(Guid id)
    {
        var query = new GetAssignmentByIdQuery(id);
        var assignment = await _sender.Send(query);

        return Ok(assignment);
    }

    /// <summary>
    /// Creates a new manager-driver assignment.
    /// Manager/driver validation uses eventual consistency — the assignment is created
    /// with the provided ManagerId and DriverId without cross-service validation calls.
    /// Publishes AssignmentCreatedIntegrationEvent to notify other microservices.
    /// </summary>
    /// <param name="command">The assignment creation data including manager ID and driver ID.</param>
    /// <returns>The unique identifier of the newly created assignment.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAssignment([FromBody] CreateAssignmentCommand command)
    {
        var assignmentId = await _sender.Send(command);

        return CreatedAtAction(nameof(GetAssignmentById), new { id = assignmentId }, assignmentId);
    }

    /// <summary>
    /// Deactivates an active assignment. Does not delete the record — sets IsActive to false.
    /// Publishes AssignmentDeactivatedIntegrationEvent to notify other microservices.
    /// </summary>
    /// <param name="id">The unique identifier of the assignment to deactivate.</param>
    [HttpPatch("deactivate/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> DeactivateAssignment(Guid id)
    {
        var command = new DeactivateAssignmentCommand(id);
        await _sender.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Permanently deletes an assignment.
    /// </summary>
    /// <param name="id">The unique identifier of the assignment to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAssignment(Guid id)
    {
        var command = new DeleteAssignmentCommand(id);
        await _sender.Send(command);

        return NoContent();
    }
}
