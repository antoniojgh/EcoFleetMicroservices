using EcoFleet.BuildingBlocks.Domain;
using EcoFleet.ManagerService.Domain.ValueObjects;

namespace EcoFleet.ManagerService.Domain.Entities;

public class Manager : Entity<ManagerId>, IAggregateRoot
{
    public FullName Name { get; private set; }
    public Email Email { get; private set; }

    // Constructor for EF Core (Id will be overwritten by the configured column mapping)
    private Manager() : base(new ManagerId(Guid.Empty))
    { }

    public Manager(FullName name, Email email)
        : base(new ManagerId(Guid.NewGuid()))
    {
        Name = name;
        Email = email;
    }

    // --- BEHAVIORS ---

    public void UpdateName(FullName name)
    {
        Name = name;
    }

    public void UpdateEmail(Email email)
    {
        Email = email;
    }
}
