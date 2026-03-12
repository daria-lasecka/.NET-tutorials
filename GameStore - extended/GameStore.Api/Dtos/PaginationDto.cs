using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace GameStore.Api.Dtos;

public record PaginationDto
{
    [DefaultValue(1)]
    public int PageNumber { get; init; } = 1;

    [DefaultValue(10)]
    public int PageSize { get; init; } = 10;
}
