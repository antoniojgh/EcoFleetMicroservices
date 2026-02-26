using System.Text.RegularExpressions;
using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Domain.Exceptions;

namespace EcoFleet.ManagerService.Domain.ValueObjects;

public class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainException("Email cannot be empty.");

        var trimmed = value.Trim();

        if (trimmed.Length > 256)
            throw new DomainException("Email is too long.");

        if (!EmailRegex.IsMatch(trimmed))
            throw new DomainException("Email format is invalid.");

        return new Email(trimmed.ToLowerInvariant());
    }

    public static Email? TryCreate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();

        if (trimmed.Length > 256)
            return null;

        if (!EmailRegex.IsMatch(trimmed))
            return null;

        return new Email(trimmed.ToLowerInvariant());
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
