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
                       .Include(game => game.Genre)
                       .AsQueryable();

        // Filtering
        if (!string.IsNullOrWhiteSpace(filter.Name))
            query = query.Where(game => game.Name.Contains(filter.Name));

        if (filter.GenreId.HasValue)
            query = query.Where(game => game.GenreId == filter.GenreId);

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
                g.Genre!.Name,
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
                game.GenreId,
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
            GenreId = newGame.GenreId,
            Price = newGame.Price,
            ReleaseDate = newGame.ReleaseDate
        };

        _dbContext.Games.Add(game);
        await _dbContext.SaveChangesAsync();

        return new GameDetailsDto(
            game.Id,
            game.Name,
            game.GenreId,
            game.Price,
            game.ReleaseDate
        );
    }

    public async Task<bool> UpdateAsync(int id, UpdateGameDto updatedGame)
    {
        var existingGame = await _dbContext.Games.FindAsync(id);

        if (existingGame is null)
        {
            return false;
        }

        existingGame.Name = updatedGame.Name;
        existingGame.GenreId = updatedGame.GenreId;
        existingGame.Price = updatedGame.Price;
        existingGame.ReleaseDate = updatedGame.ReleaseDate;

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var deleted = await _dbContext.Games
            .Where(game => game.Id == id)
            .ExecuteDeleteAsync();

        return deleted > 0;
    }

}