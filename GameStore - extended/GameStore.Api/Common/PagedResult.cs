namespace GameStore.Api.Common;

public record PagedResult<T>(
    IEnumerable<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount
)
{
    public int TotalPages =>
        (int)Math.Ceiling((double)TotalCount / PageSize);

    public bool HasPrevious =>
        PageNumber > 1;

    public bool HasNext =>
        PageNumber < TotalPages;
}