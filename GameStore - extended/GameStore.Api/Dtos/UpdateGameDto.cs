using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record UpdateGameDto(
    [Required, StringLength(50)] string Name,
    [Range(1, 50)] int PublisherId,
    List<int> GenreIds,
    [Range(1, 100)] decimal Price,
    DateOnly ReleaseDate
);
