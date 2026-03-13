namespace GameStore.Api.Dtos;

public record PublisherDetailsDto(
    int Id,
    string Name,
    List<GameEssentialDataDto> Games
);
