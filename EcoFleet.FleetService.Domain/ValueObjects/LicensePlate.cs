using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Domain.Exceptions;

namespace EcoFleet.FleetService.Domain.ValueObjects;

public class LicensePlate : ValueObject
{
    public string Value { get; }

    private LicensePlate(string value) => Value = value;

    public static LicensePlate Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("License plate cannot be empty.");

        return new LicensePlate(value.ToUpperInvariant());
    }

    public static LicensePlate? TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return new LicensePlate(value.ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
