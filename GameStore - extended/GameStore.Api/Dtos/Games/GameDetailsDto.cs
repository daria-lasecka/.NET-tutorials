namespace GameStore.Api.Dtos;

public record GameDetailsDto(
    int Id,
    string Name,
    int PublisherId,
    List<int> GenreIds,
    decimal Price,
    DateOnly ReleaseDate
);
