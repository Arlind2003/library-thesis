namespace Caktoje.Data.Resources.Common;

public class PaginatedResource<T>
{
    public required IEnumerable<T> Items { get; set; }
    public required int TotalPages { get; set; }
}