namespace EcoFleet.FleetService.Infrastructure.Projections;

public class VehicleReadModel
{
    public Guid Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public Guid? CurrentDriverId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
