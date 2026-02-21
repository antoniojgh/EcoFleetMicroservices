using EcoFleet.Application.Interfaces.Data;
using EcoFleet.Domain.Common;
using EcoFleet.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EcoFleet.BuildingBlocks.Infrastructure.Repositories
{
    public class Repository<T, TId> : IRepository<T, TId> where T : Entity<TId>, IAggregateRoot
                                                          where TId : notnull
    {
        private readonly ApplicationDbContext _context;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<T?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(e => e.Id.Equals(id), cancellationToken);
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().ToListAsync(cancellationToken);
        }

        public async Task<int> GetTotalNumberOfRecords(CancellationToken cancellationToken = default)
        {
            return await _context.Set<T>().CountAsync(cancellationToken);
        }

        public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        {
            await _context.Set<T>().AddAsync(entity, cancellationToken);
        }

        public Task Update(T entity, CancellationToken cancellationToken = default)
        {
            _context.Set<T>().Update(entity);
            return Task.CompletedTask;
        }

        public Task Delete(T entity, CancellationToken cancellationToken = default)
        {
            _context.Set<T>().Remove(entity);
            return Task.CompletedTask;
        }
    }
}
