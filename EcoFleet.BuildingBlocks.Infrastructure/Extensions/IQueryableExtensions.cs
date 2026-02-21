namespace EcoFleet.BuildingBlocks.Infrastructure.Extensions
{
    public static class IQueryableExtensions
    {
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable, int page, int recordsByPage)
        {
            return queryable
                .Skip((page - 1) * recordsByPage)
                .Take(recordsByPage);
        }
    }
}
