namespace GameStore.Api.Dtos;

// A DTO is a contract between the client and the server since it represents
//  a shared agreement about how the data will be transfered and used.

public record GameSummaryDto(
    int Id,
    string Name,
    string Publisher,
    List<string> Genre,
    decimal Price,
    DateOnly ReleaseDate
);
