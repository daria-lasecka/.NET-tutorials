using GameStore.Api.Common;
using GameStore.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGameById";
    const string GamesTag = "Games";

    public static void MapGamesEndpoints(this WebApplication app)
    {

        var group = app.MapGroup("/games");

        // GET /games
        group.MapGet("/", async ([AsParameters] GameFilterDto filter, [FromServices] IGameService gameService) =>
        {
            var result = await gameService.GetGamesAsync(filter);
            return Results.Ok(result);
        })
        .WithSummary("Get Games")
        .WithDescription("Returns paginated list of games.")
        .Produces<PagedResult<GameSummaryDto>>(StatusCodes.Status200OK)
        .WithTags(GamesTag);

        // GET /games/1
        group.MapGet("/{id}", async (int id, [FromServices] IGameService gameService) =>
        {
            var game = await gameService.GetByIdAsync(id);

            return game is null ? Results.NotFound() : Results.Ok(game);
        })
           .WithName(GetGameEndpointName) // TODO: add this kind of documentation to all endpoints 
           .WithSummary("Get game by ID")
           .WithDescription("Returns a single game based on their unique identifier. Returns 404 if the game doesn't exist.")
           .Produces<GameDetailsDto>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound)
           .WithTags(GamesTag);

        // POST /games
        group.MapPost("/", async (CreateGameDto newGame, [FromServices] IGameService gameService) =>
        {
            var createdGame = await gameService.CreateAsync(newGame);

            return Results.CreatedAtRoute(GetGameEndpointName, new { id = createdGame.Id }, createdGame);
        })
            .WithName("CreateGame")
            .WithSummary("Create game")
            .WithDescription("Creates a game object and returns it with location.")
            .Produces<GameDetailsDto>(StatusCodes.Status201Created)
            .WithTags(GamesTag);

        // PUT /games/1
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, [FromServices] IGameService gameService) =>
        {
            var existingGame = await gameService.UpdateAsync(id, updatedGame);

            return existingGame ? Results.NoContent() : Results.NotFound();
        })
        .WithSummary("Update game")
        .WithDescription("Updated a game basedon their unique identifier.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags(GamesTag);

        // DELETE /games/1
        group.MapDelete("/{id}", async (int id, [FromServices] IGameService gameService) =>
        {
            var deletedGame = await gameService.DeleteAsync(id);

            return deletedGame ? Results.NoContent() : Results.NotFound();
        })
        .WithSummary("Delete game")
        .WithDescription("Deletes a game basedon their unique identifier.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags(GamesTag);
    }
}
