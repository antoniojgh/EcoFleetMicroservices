using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.ManagerService.Application.DTOs;
using EcoFleet.ManagerService.Application.UseCases.Commands.CreateManager;
using EcoFleet.ManagerService.Application.UseCases.Commands.DeleteManager;
using EcoFleet.ManagerService.Application.UseCases.Commands.UpdateManager;
using EcoFleet.ManagerService.Application.UseCases.Queries.GetAllManagers;
using EcoFleet.ManagerService.Application.UseCases.Queries.GetManagerById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoFleet.ManagerService.API.Controllers;

/// <summary>
/// Manages managers — simple CRUD operations.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
public class ManagersController : ControllerBase
{
    private readonly ISender _sender;

    public ManagersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves a paginated list of managers with optional filters.
    /// </summary>
    /// <param name="query">Optional filters: name, email, and pagination parameters.</param>
    /// <returns>A paginated list of managers matching the specified filters.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedDTO<ManagerDetailDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllManagers([FromQuery] GetAllManagersQuery query)
    {
        var result = await _sender.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single manager by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the manager.</param>
    /// <returns>The manager details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ManagerDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetManagerById(Guid id)
    {
        var query = new GetManagerByIdQuery(id);
        var manager = await _sender.Send(query);

        return Ok(manager);
    }

    /// <summary>
    /// Creates a new manager.
    /// </summary>
    /// <param name="command">The manager creation data including name and email.</param>
    /// <returns>The unique identifier of the newly created manager.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateManager([FromBody] CreateManagerCommand command)
    {
        var managerId = await _sender.Send(command);

        return CreatedAtAction(nameof(CreateManager), new { id = managerId }, managerId);
    }

    /// <summary>
    /// Updates an existing manager's name and email.
    /// </summary>
    /// <param name="id">The unique identifier of the manager to update.</param>
    /// <param name="command">The updated manager data.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateManager(Guid id, [FromBody] UpdateManagerCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("The ID in the URL does not match the ID in the body.");
        }

        await _sender.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Permanently deletes a manager.
    /// </summary>
    /// <param name="id">The unique identifier of the manager to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteManager(Guid id)
    {
        var command = new DeleteManagerCommand(id);
        await _sender.Send(command);

        return NoContent();
    }
}
