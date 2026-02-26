namespace EcoFleet.ManagerService.Application.DTOs;

public record FilterManagerDTO
{
    public int Page { get; init; } = 1;
    public int RecordsByPage { get; init; } = 10;
    public Guid? Id { get; init; }
    public string? FirstName { get; init; }
    public string? LastName { get; init; }
    public string? Email { get; init; }
}
