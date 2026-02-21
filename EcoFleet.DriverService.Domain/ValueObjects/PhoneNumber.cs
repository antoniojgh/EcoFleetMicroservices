using System.Text.RegularExpressions;
using EcoFleet.Domain.Common;
using EcoFleet.Domain.Exceptions;

namespace EcoFleet.DriverService.Domain.ValueObjects
{
    public class PhoneNumber : ValueObject
    {
        private static readonly Regex PhoneRegex = new(
            @"^\+?[\d\s\-()]{7,20}$",
            RegexOptions.Compiled);

        public string Value { get; }

        private PhoneNumber(string value) => Value = value;

        public static PhoneNumber Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Phone number cannot be empty.");

            var trimmed = value.Trim();

            if (!PhoneRegex.IsMatch(trimmed))
                throw new DomainException("Phone number format is invalid.");

            return new PhoneNumber(trimmed);
        }

        public static PhoneNumber? TryCreate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var trimmed = value.Trim();

            if (!PhoneRegex.IsMatch(trimmed))
                return null;

            return new PhoneNumber(trimmed);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
