namespace EcoFleet.BuildingBlocks.Application.Common
{
    public class PaginatedDTO<T>
    {
        public int TotalCount { get; set; }
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    }
}
