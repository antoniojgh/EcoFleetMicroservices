using EcoFleet.BuildingBlocks.Domain;

namespace EcoFleet.DriverService.Domain.Entities
{
    public class Driver : Entity<DriverId>, IAggregateRoot
    {
        public FullName Name { get; private set; }
        public DriverLicense License { get; private set; }
        public Email Email { get; private set; }
        public PhoneNumber? PhoneNumber { get; private set; }
        public DateTime? DateOfBirth { get; private set; }
        public DriverStatus Status { get; private set; }

        public Guid? AssignedVehicleId { get; private set; }

        // Constructor for EF Core
        private Driver() : base(new DriverId(Guid.NewGuid()))
        { }

        // Public Constructor: available driver without vehicle
        public Driver(FullName name, DriverLicense license, Email email, PhoneNumber? phoneNumber = null, DateTime? dateOfBirth = null)
            : base(new DriverId(Guid.NewGuid()))
        {
            Name = name;
            License = license;
            Email = email;
            PhoneNumber = phoneNumber;
            DateOfBirth = dateOfBirth;
            Status = DriverStatus.Available;
        }

        // Public Constructor: driver assigned to a vehicle (on duty)
        public Driver(FullName name, DriverLicense license, Email email, VehicleId vehicleId, PhoneNumber? phoneNumber = null, DateTime? dateOfBirth = null)
            : base(new DriverId(Guid.NewGuid()))
        {
            Name = name;
            License = license;
            Email = email;
            PhoneNumber = phoneNumber;
            DateOfBirth = dateOfBirth;
            CurrentVehicleId = vehicleId;
            Status = DriverStatus.OnDuty;
        }

        // --- BEHAVIORS (Methods, not Setters) ---

        public void UpdateName(FullName name)
        {
            Name = name;
        }

        public void UpdateLicense(DriverLicense license)
        {
            License = license;
        }

        public void UpdateEmail(Email email)
        {
            Email = email;
        }

        public void UpdatePhoneNumber(PhoneNumber? phoneNumber)
        {
            PhoneNumber = phoneNumber;
        }

        public void UpdateDateOfBirth(DateTime? dateOfBirth)
        {
            DateOfBirth = dateOfBirth;
        }

        public void AssignVehicle(VehicleId vehicleId)
        {
            if (Status == DriverStatus.Suspended)
            {
                throw new DomainException("Cannot assign a vehicle to a suspended driver.");
            }

            CurrentVehicleId = vehicleId;
            Status = DriverStatus.OnDuty;
        }

        public void UnassignVehicle()
        {
            if (Status == DriverStatus.Suspended)
                return;

            CurrentVehicleId = null;
            Status = DriverStatus.Available;
        }

        public void Suspend()
        {
            if (Status == DriverStatus.OnDuty)
            {
                throw new DomainException("Cannot suspend a driver currently on duty.");
            }

            Status = DriverStatus.Suspended;
            CurrentVehicleId = null;

            AddDomainEvent(new DriverSuspendedEvent(Id, DateTime.UtcNow));
        }

        public void Reinstate()
        {
            if (Status != DriverStatus.Suspended)
            {
                throw new DomainException("Only suspended drivers can be reinstated.");
            }

            Status = DriverStatus.Available;

            AddDomainEvent(new DriverReinstatedEvent(Id, DateTime.UtcNow));
        }
    }
}
