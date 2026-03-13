using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record CreatePublisherDto(
    [Required, StringLength(50, MinimumLength = 1)] string Name
);
