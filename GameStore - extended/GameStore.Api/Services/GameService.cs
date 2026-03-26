using GameStore.Api.Common;
using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

public class GameService : IGameService
{
    private readonly GameStoreContext _dbContext;

    public GameService(GameStoreContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<GameSummaryDto>> GetGamesAsync(GameFilterDto filter, PaginationDto pagination)
    {
        var query = _dbContext.Games
                       .Include(g => g.Publisher)
                       .Include(g => g.GameGenres)
                        .ThenInclude(gg => gg.Genre)
                       .AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(game => game.Name.Contains(filter.Name));

        if (filter.GenreId.HasValue)
            query = query.Where(game => game.GameGenres.Any(gg => gg.GenreId == filter.GenreId.Value));

        if (filter.PublisherId.HasValue)
            query = query.Where(game => game.PublisherId == filter.PublisherId.Value);

        if (filter.MinPrice.HasValue)
            query = query.Where(game => game.Price >= filter.MinPrice);

        if (filter.MaxPrice.HasValue)
            query = query.Where(game => game.Price <= filter.MaxPrice);

        if (filter.ReleasedAfter.HasValue)
            query = query.Where(game => game.ReleaseDate >= filter.ReleasedAfter);

        if (filter.ReleasedBefore.HasValue)
            query = query.Where(game => game.ReleaseDate <= filter.ReleasedBefore);

        // Pagination
        var pageNumber = Math.Max(pagination.PageNumber, 1);
        var pageSize = Math.Clamp(pagination.PageSize, 1, 100);

        var totalCount = await query.CountAsync();

        // TODO: handle custom order by (default to name)
        // TODO: handle custom asc/desc
        var items = await query
            .OrderBy(game => game.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(g => new GameSummaryDto(
                g.Id,
                g.Name,
                g.Publisher!.Name, // TODO: might be unknow publisher?
                g.GameGenres.Select(gg => gg.Genre.Name).ToList(),
                g.Price,
                g.ReleaseDate
            ))
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<GameSummaryDto>(
            items,
            pageNumber,
            pageSize,
            totalCount
        );
    }

    public async Task<GameDetailsDto?> GetByIdAsync(int id)
    {
        return await _dbContext.Games
            .Where(game => game.Id == id)
            .Select(game => new GameDetailsDto(
                game.Id,
                game.Name,
                game.PublisherId,
                game.GameGenres.Select(gg => gg.GenreId).ToList(),
                game.Price,
                game.ReleaseDate
            ))
            .AsNoTracking()
            .FirstOrDefaultAsync();
    }


    public async Task<GameDetailsDto> CreateAsync(CreateGameDto newGame)
    {

        Game game = new()
        {
            Name = newGame.Name,
            PublisherId = newGame.PublisherId,
            Price = newGame.Price,
            ReleaseDate = newGame.ReleaseDate
        };

        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync();

        // Assign genress
        if (newGame.GenreIds != null && newGame.GenreIds.Count != 0)
        {
            var gameGenres = newGame.GenreIds.Select(genreId => new GameGenre
            {
                GameId = game.Id,
                GenreId = genreId
            });
            _dbContext.GameGenres.AddRange(gameGenres);
            await _dbContext.SaveChangesAsync();
        }

        // TODO: handle error 
        return await GetByIdAsync(game.Id) ?? throw new InvalidOperationException("Game creation failed");
    }

    public async Task<bool> UpdateAsync(int id, UpdateGameDto updatedGame)
    {
        var existingGame = await _dbContext.Games
            .Include(g => g.GameGenres)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (existingGame is null)
        {
            return false;
        }

        existingGame.Name = updatedGame.Name;
        existingGame.PublisherId = updatedGame.PublisherId;
        existingGame.Price = updatedGame.Price;
        existingGame.ReleaseDate = updatedGame.ReleaseDate;

        if (updatedGame.GenreIds != null)
        {
            _dbContext.GameGenres.RemoveRange(existingGame.GameGenres);

            var newGameGenres = updatedGame.GenreIds.Select(genreId => new GameGenre
            {
                GameId = existingGame.Id,
                GenreId = genreId
            });
            _dbContext.GameGenres.AddRange(newGameGenres);
        }

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // TODO: delete genres too
        var deleted = await _dbContext.Games
            .Where(game => game.Id == id)
            .ExecuteDeleteAsync();

        return deleted > 0;
    }

}