using System.ComponentModel.DataAnnotations;

namespace GameStore.Api.Dtos;

public record UpdatePublisherDto(
    [Required, StringLength(50, MinimumLength = 1)] string Name
);
