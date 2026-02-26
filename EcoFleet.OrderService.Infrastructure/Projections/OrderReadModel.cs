namespace EcoFleet.OrderService.Infrastructure.Projections;

public class OrderReadModel
{
    public Guid Id { get; set; }
    public Guid DriverId { get; set; }
    public string Status { get; set; } = string.Empty;
    public double PickUpLatitude { get; set; }
    public double PickUpLongitude { get; set; }
    public double DropOffLatitude { get; set; }
    public double DropOffLongitude { get; set; }
    public decimal Price { get; set; }
    public string? CancellationReason { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
