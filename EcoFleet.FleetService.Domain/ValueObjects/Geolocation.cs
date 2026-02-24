using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Domain.Exceptions;

namespace EcoFleet.FleetService.Domain.ValueObjects;

public class Geolocation : ValueObject
{
    public double Latitude { get; }
    public double Longitude { get; }

    // Required for EF Core
    private Geolocation() { }

    private Geolocation(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
            throw new DomainException("Invalid Latitude.");

        if (longitude < -180 || longitude > 180)
            throw new DomainException("Invalid Longitude.");

        Latitude = latitude;
        Longitude = longitude;
    }

    public static Geolocation Create(double lat, double lon) => new(lat, lon);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Latitude;
        yield return Longitude;
    }
}
