using GameStore.Api.Common;
using GameStore.Api.Dtos;

public interface IPublisherService
{
    Task<PagedResult<PublisherDetailsDto>> GetPublishersAsync(PaginationDto pagination);
    Task<PublisherDetailsDto?> GetByIdAsync(int id);
    Task<PublisherDto> CreateAsync(CreatePublisherDto dto);
    Task<bool> UpdateAsync(int id, UpdatePublisherDto dto);
    Task<bool> DeleteAsync(int id);
}