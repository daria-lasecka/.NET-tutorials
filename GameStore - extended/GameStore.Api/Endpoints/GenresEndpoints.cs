using GameStore.Api.Common;
using GameStore.Api.Data;
using GameStore.Api.Dtos;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Endpoints;

public static class GenresEndpoints
{
    const string GenresTag = "Genres";


    public static void MapGenresEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/genres");

        // GET /genres
        group.MapGet("/", async ([AsParameters] PaginationDto pagination, GameStoreContext dbContext) =>
        {
            var query = dbContext.Genres.AsQueryable();

            // Pagination
            var pageNumber = Math.Max(pagination.PageNumber, 1);
            var pageSize = Math.Clamp(pagination.PageSize, 1, 100);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(genre => genre.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new GenreDto(
                    g.Id,
                    g.Name
                ))
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<GenreDto>(
                items,
                pageNumber,
                pageSize,
                totalCount
            );
        }
        // await dbContext.Genres
        //                .Select(genre => new GenreDto(genre.Id, genre.Name))
        //                .AsNoTracking()
        //                .ToListAsync()
        )
        .WithSummary("Get Genres")
        .WithDescription("Returns paginated list of genres.")
        .Produces<PagedResult<GenreDto>>(StatusCodes.Status200OK)
        .WithTags(GenresTag);
    }

}
