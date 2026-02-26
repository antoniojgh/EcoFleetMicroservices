namespace EcoFleet.OrderService.Application.DTOs;

public record FilterOrderDTO
{
    public int Page { get; init; } = 1;
    public int RecordsByPage { get; init; } = 10;
    public Guid? Id { get; init; }
    public Guid? DriverId { get; init; }
    public string? Status { get; init; }
}
