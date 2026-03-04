using GameStore.Api.Common;
using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GamesEndpoints
{
    const string GetGameEndpointName = "GetGameById";
    const string GamesTag = "Games";

    public static void MapGamesEndpoints(this WebApplication app)
    {

        var group = app.MapGroup("/games");

        // GET /games
        // group.MapGet("/", async (GameStoreContext dbContext) //[AsParameters] GameFilter filter,
        //     => await dbContext.Games
        //                     .Include(game => game.Genre)
        //                     .Select(game => new GameSummaryDto(
        //                         game.Id,
        //                         game.Name,
        //                         game.Genre!.Name,
        //                         game.Price,
        //                         game.ReleaseDate
        //                     ))
        //                     .AsNoTracking()
        //                     .ToListAsync());
        group.MapGet("/", async (
            [AsParameters] GameFilter filter,
            IGameService service) =>
        {
            var result = await service.GetGamesAsync(filter);
            return Results.Ok(result);
        })
        .Produces<PagedResult<GameSummaryDto>>(StatusCodes.Status200OK);

        // GET /games/1
        group.MapGet("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            var game = await dbContext.Games.FindAsync(id);

            return game is null ? Results.NotFound() : Results.Ok(
                new GameDetailsDto(
                    game.Id,
                    game.Name,
                    game.GenreId,
                    game.Price,
                    game.ReleaseDate
                )
            );
        })
           .WithName(GetGameEndpointName) // TODO: add this kind of documentation to all endpoints 
           .WithSummary("Get game by ID")
           .WithDescription("Returns a single game based on their unique identifier. Returns 404 if the game doesn't exist.")
           .Produces<GameDetailsDto>(StatusCodes.Status200OK) // TODO: check which type should be here, probably GameDetailsDto instead of Game, but I might be wrong
           .Produces(StatusCodes.Status404NotFound)
           .WithTags(GamesTag);

        // POST /games
        group.MapPost("/", async (CreateGameDto newGame, GameStoreContext dbContext) =>
        {
            Game game = new()
            {
                Name = newGame.Name,
                GenreId = newGame.GenreId,
                Price = newGame.Price,
                ReleaseDate = newGame.ReleaseDate
            };

            dbContext.Games.Add(game);
            await dbContext.SaveChangesAsync();

            GameDetailsDto gameDto = new(
                game.Id,
                game.Name,
                game.GenreId,
                game.Price,
                game.ReleaseDate
            );

            return Results.CreatedAtRoute(GetGameEndpointName, new { id = gameDto.Id }, gameDto);
        })
            .WithName("CreateGame")
            .WithSummary("Create game")
            .WithDescription("Creates a game object and returns it with location.")
            .Produces<GameDetailsDto>(StatusCodes.Status201Created)
            .WithTags(GamesTag);

        // PUT /games/1
        group.MapPut("/{id}", async (int id, UpdateGameDto updatedGame, GameStoreContext dbContext) =>
        {
            var existingGame = await dbContext.Games.FindAsync(id);

            if (existingGame is null)
            {
                return Results.NotFound();
            }

            existingGame.Name = updatedGame.Name;
            existingGame.GenreId = updatedGame.GenreId;
            existingGame.Price = updatedGame.Price;
            existingGame.ReleaseDate = updatedGame.ReleaseDate;

            await dbContext.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE /games/1
        group.MapDelete("/{id}", async (int id, GameStoreContext dbContext) =>
        {
            await dbContext.Games
                           .Where(game => game.Id == id)
                           .ExecuteDeleteAsync();


            return Results.NoContent();
        });
    }
}
