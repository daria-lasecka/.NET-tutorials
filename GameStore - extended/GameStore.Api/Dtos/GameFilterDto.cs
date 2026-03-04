using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record GameFilter(
    string? Name,
    //string? Genre,
    int? GenreId,
    [Range(0, double.MaxValue)] decimal? MinPrice,
    [Range(0, double.MaxValue)] decimal? MaxPrice,
    DateOnly? ReleasedAfter,
    DateOnly? ReleasedBefore,
    [Range(1, int.MaxValue)] int PageNumber = 1,
    [Range(1, 100)] int PageSize = 10
);