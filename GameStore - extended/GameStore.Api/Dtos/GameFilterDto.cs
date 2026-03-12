using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace GameStore.Api.Dtos;

public record GameFilterDto
{
    public string? Name { get; init; } // TODO: add info that it's case sensitive
    public int? GenreId { get; init; }
    public decimal? MinPrice { get; init; }
    public decimal? MaxPrice { get; init; }
    public DateOnly? ReleasedAfter { get; init; }
    public DateOnly? ReleasedBefore { get; init; }
}
