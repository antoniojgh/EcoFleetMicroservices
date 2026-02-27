using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.BuildingBlocks.Domain.Exceptions;

namespace EcoFleet.DriverService.Domain.ValueObjects
{
    public class FullName : ValueObject
    {
        public string FirstName { get; }
        public string LastName { get; }

        private FullName(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public static FullName Create(string firstName, string lastName)
        {
            if (string.IsNullOrWhiteSpace(firstName))
                throw new DomainException("First name cannot be empty.");

            if (string.IsNullOrWhiteSpace(lastName))
                throw new DomainException("Last name cannot be empty.");

            if (firstName.Trim().Length > 100)
                throw new DomainException("First name is too long.");

            if (lastName.Trim().Length > 100)
                throw new DomainException("Last name is too long.");

            return new FullName(firstName.Trim(), lastName.Trim());
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return FirstName;
            yield return LastName;
        }
    }
}
