using EcoFleet.Domain.Common;
using EcoFleet.Domain.Exceptions;

namespace EcoFleet.DriverService.Domain.ValueObjects
{
    public class DriverLicense : ValueObject
    {
        public string Value { get; }

        private DriverLicense(string value) => Value = value;

        public static DriverLicense Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new DomainException("Driver license cannot be empty.");

            if (value.Trim().Length > 20)
                throw new DomainException("Driver license is too long.");

            return new DriverLicense(value.Trim().ToUpperInvariant());
        }

        /// <summary>
        /// Attempts to create a new instance of the DriverLicense class from the specified string representation.
        /// </summary>
        /// <remarks>The input string is trimmed and converted to uppercase before creating the
        /// DriverLicense instance. If the input does not meet the required criteria, this method returns null instead
        /// of throwing an exception.</remarks>
        /// <param name="value">The string representation of the driver's license. The value must not be null, empty, or consist only of
        /// white-space characters, and must not exceed 20 characters in length.</param>
        /// <returns>A DriverLicense instance if the input is valid; otherwise, null.</returns>
        public static DriverLicense? TryCreate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            if (value.Trim().Length > 20)
                return null;

            return new DriverLicense(value.Trim().ToUpperInvariant());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Value;
        }
    }
}
