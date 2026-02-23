using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.DriverService.Application.DTOs;
using EcoFleet.DriverService.Application.UseCases.Commands.CreateDriver;
using EcoFleet.DriverService.Application.UseCases.Commands.DeleteDriver;
using EcoFleet.DriverService.Application.UseCases.Commands.ReinstateDriver;
using EcoFleet.DriverService.Application.UseCases.Commands.SuspendDriver;
using EcoFleet.DriverService.Application.UseCases.Commands.UpdateDriver;
using EcoFleet.DriverService.Application.UseCases.Queries.GetAllDrivers;
using EcoFleet.DriverService.Application.UseCases.Queries.GetDriverById;
using MediatR;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;

namespace EcoFleet.DriverService.API.Controllers;

/// <summary>
/// Manages drivers — CRUD operations, vehicle assignments, suspension, and reinstatement.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
public class DriversController : ControllerBase
{
    private readonly ISender _sender;

    public DriversController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves a paginated list of drivers with optional filters.
    /// </summary>
    /// <param name="query">Optional filters: name, license, email, phone, date of birth range, status, vehicle, and pagination parameters.</param>
    /// <returns>A paginated list of drivers matching the specified filters.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedDTO<DriverDetailDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllDrivers([FromQuery] GetAllDriversQuery query)
    {
        var result = await _sender.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single driver by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the driver.</param>
    /// <returns>The driver details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(DriverDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDriverById(Guid id)
    {
        var query = new GetDriverByIdQuery(id);
        var driver = await _sender.Send(query);

        return Ok(driver);
    }

    /// <summary>
    /// Creates a new driver and optionally assigns them to a vehicle.
    /// </summary>
    /// <param name="command">The driver creation data including name, license, email, and optional phone number, date of birth, and vehicle ID.</param>
    /// <returns>The unique identifier of the newly created driver.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateDriver([FromBody] CreateDriverCommand command)
    {
        var driverId = await _sender.Send(command);

        return CreatedAtAction(nameof(CreateDriver), new { id = driverId }, driverId);
    }

    /// <summary>
    /// Updates an existing driver's name, license, email, phone number, date of birth, and vehicle assignment.
    /// </summary>
    /// <param name="id">The unique identifier of the driver to update.</param>
    /// <param name="command">The updated driver data.</param>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateDriver(Guid id, [FromBody] UpdateDriverCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("The ID in the URL does not match the ID in the body.");
        }

        await _sender.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Suspends an available driver. Fails if the driver is currently on duty.
    /// </summary>
    /// <param name="id">The unique identifier of the driver to suspend.</param>
    [HttpPatch("suspend/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> SuspendDriver(Guid id)
    {
        var command = new SuspendDriverCommand(id);
        await _sender.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Reinstates a suspended driver back to available status.
    /// </summary>
    /// <param name="id">The unique identifier of the driver to reinstate.</param>
    [HttpPatch("reinstate/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> ReinstateDriver(Guid id)
    {
        var command = new ReinstateDriverCommand(id);
        await _sender.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Permanently deletes a driver.
    /// </summary>
    /// <param name="id">The unique identifier of the driver to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDriver(Guid id)
    {
        var command = new DeleteDriverCommand(id);
        await _sender.Send(command);

        return NoContent();
    }
}
