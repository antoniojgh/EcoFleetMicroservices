using EcoFleet.BuildingBlocks.Domain;

namespace EcoFleet.BuildingBlocks.Application.Interfaces
{
    public interface IRepository<T, TId> where T : Entity<TId>, IAggregateRoot
    {
        // We only expose what we strictly need for the logic
        Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<int> GetTotalNumberOfRecords(CancellationToken cancellationToken = default);
        Task AddAsync(T entity, CancellationToken cancellationToken = default);
        Task Update(T entity, CancellationToken cancellationToken = default);
        Task Delete(T entity, CancellationToken cancellationToken = default);
    }
}
