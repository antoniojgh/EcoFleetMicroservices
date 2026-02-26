using EcoFleet.BuildingBlocks.Application.Common;
using EcoFleet.OrderService.Application.DTOs;
using EcoFleet.OrderService.Application.UseCases.Commands.CancelOrder;
using EcoFleet.OrderService.Application.UseCases.Commands.CompleteOrder;
using EcoFleet.OrderService.Application.UseCases.Commands.CreateOrder;
using EcoFleet.OrderService.Application.UseCases.Commands.StartOrder;
using EcoFleet.OrderService.Application.UseCases.Queries.GetAllOrders;
using EcoFleet.OrderService.Application.UseCases.Queries.GetOrderById;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EcoFleet.OrderService.API.Controllers;

/// <summary>
/// Manages orders — CRUD operations and order lifecycle (create, start, complete, cancel).
/// Uses event sourcing with Marten for state management and MassTransit for integration events.
/// DriverId is a primitive Guid — no cross-boundary strong typing.
/// </summary>
[Route("api/v1/[controller]")]
[ApiController]
[Produces("application/json")]
public class OrdersController : ControllerBase
{
    private readonly ISender _sender;

    public OrdersController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Retrieves a paginated list of orders with optional filters.
    /// </summary>
    /// <param name="query">Optional filters: driverId, status, and pagination parameters.</param>
    /// <returns>A paginated list of orders matching the specified filters.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedDTO<OrderDetailDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllOrders([FromQuery] GetAllOrdersQuery query)
    {
        var result = await _sender.Send(query);

        return Ok(result);
    }

    /// <summary>
    /// Retrieves a single order by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the order.</param>
    /// <returns>The order details.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(OrderDetailDTO), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderById(Guid id)
    {
        var query = new GetOrderByIdQuery(id);
        var order = await _sender.Send(query);

        return Ok(order);
    }

    /// <summary>
    /// Creates a new order assigned to a driver.
    /// Driver validation uses eventual consistency — the order is created with the provided DriverId.
    /// Publishes OrderCreatedIntegrationEvent to notify other microservices.
    /// </summary>
    /// <param name="command">The order creation data including driver ID, geolocations, and price.</param>
    /// <returns>The unique identifier of the newly created order.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
    {
        var orderId = await _sender.Send(command);

        return CreatedAtAction(nameof(CreateOrder), new { id = orderId }, orderId);
    }

    /// <summary>
    /// Starts a pending order, transitioning its status to InProgress.
    /// </summary>
    /// <param name="id">The unique identifier of the order to start.</param>
    [HttpPatch("start/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> StartOrder(Guid id)
    {
        var command = new StartOrderCommand(id);
        await _sender.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Completes an in-progress order.
    /// Publishes OrderCompletedIntegrationEvent to notify other microservices (e.g. NotificationService).
    /// </summary>
    /// <param name="id">The unique identifier of the order to complete.</param>
    [HttpPatch("complete/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CompleteOrder(Guid id)
    {
        var command = new CompleteOrderCommand(id);
        await _sender.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Cancels a pending or in-progress order.
    /// Publishes OrderCancelledIntegrationEvent to notify other microservices.
    /// </summary>
    /// <param name="id">The unique identifier of the order to cancel.</param>
    /// <param name="request">Optional cancellation reason.</param>
    [HttpPatch("cancel/{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
    {
        await _sender.Send(new CancelOrderCommand(id, request.CancellationReason));

        return NoContent();
    }
}
