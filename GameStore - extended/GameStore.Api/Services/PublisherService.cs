using GameStore.Api.Common;
using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;
using Microsoft.EntityFrameworkCore;

public class PublisherService : IPublisherService
{
    private readonly GameStoreContext _dbContext;

    public PublisherService(GameStoreContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<PublisherDetailsDto>> GetPublishersAsync(PaginationDto pagination)
    {
        var query = _dbContext.Publishers
                            .Include(p => p.Games)
                            .AsQueryable();

        var pageNumber = Math.Max(pagination.PageNumber, 1);
        var pageSize = Math.Clamp(pagination.PageSize, 1, 100);

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PublisherDetailsDto(
                p.Id,
                p.Name,
                p.Games.Select(g => new GameEssentialDataDto(
                    g.Id,
                    g.Name
                )).ToList()
            ))
            .AsNoTracking()
            .ToListAsync();

        return new PagedResult<PublisherDetailsDto>(items, pageNumber, pageSize, totalCount);

    }

    public async Task<PublisherDetailsDto?> GetByIdAsync(int id)
    {
        var publisher = await _dbContext.Publishers
                                .Include(p => p.Games)
                                .FirstOrDefaultAsync(p => p.Id == id);

        if (publisher == null) return null;

        return new PublisherDetailsDto(
            publisher.Id,
            publisher.Name,
            [.. publisher.Games.Select(g => new GameEssentialDataDto(
                g.Id,
                g.Name
            ))]
        );
    }

    public async Task<PublisherDto> CreateAsync(CreatePublisherDto newPublisher)
    {
        Publisher publisher = new()
        {
            Name = newPublisher.Name,
        };

        _dbContext.Publishers.Add(publisher);
        return new PublisherDto(publisher.Id, publisher.Name);
    }

    public async Task<bool> UpdateAsync(int id, UpdatePublisherDto updatedPublisher)
    {
        var publisher = await _dbContext.Publishers
                                .FirstOrDefaultAsync(p => p.Id == id);

        if (publisher is null)
        {
            return false;
        }

        publisher.Name = updatedPublisher.Name;

        // TODO: update publisher's games (separate endpoint?)

        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var hasGames = await _dbContext.Games.AnyAsync(g => g.PublisherId == id);
        // Don't delete if has games under it
        if (hasGames)
        {
            // throw new InvalidOperationException("Cannot delete publisher with existing games.");
            return false;
        }

        var deleted = await _dbContext.Publishers
            .Where(publisher => publisher.Id == id)
            .ExecuteDeleteAsync();

        return deleted > 0;
    }


}