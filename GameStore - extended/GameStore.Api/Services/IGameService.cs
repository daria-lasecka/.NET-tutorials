using GameStore.Api.Common;
using GameStore.Api.Dtos;

public interface IGameService
{
    Task<PagedResult<GameSummaryDto>> GetGamesAsync(GameFilter filter);
    Task<GameDetailsDto?> GetByIdAsync(int id);
    Task<GameDetailsDto> CreateAsync(CreateGameDto dto);
    Task<bool> UpdateAsync(int id, UpdateGameDto dto);
    Task<bool> DeleteAsync(int id);
}