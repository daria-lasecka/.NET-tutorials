using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record CreateGameDto(
    [Required, StringLength(50, MinimumLength = 1)] string Name,
    [Range(1, 50)] int PublisherId,
    List<int> GenreIds,
    [Range(1, 100)] decimal Price,
    DateOnly ReleaseDate
);
