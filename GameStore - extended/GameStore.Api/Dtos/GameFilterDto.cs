using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record GameFilterDto(
    string? Name,
    string? Genre,
    int? GenreId,
    decimal? MinPrice,
    decimal? MaxPrice,
    DateOnly? ReleasedAfter,
    DateOnly? ReleasedBefore,
    int? PageNumber,
    int? PageSize
);
