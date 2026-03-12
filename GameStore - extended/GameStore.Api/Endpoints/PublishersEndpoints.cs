using GameStore.Api.Common;
using GameStore.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GameStore.Api.Endpoints;

public static class PublishersEndpoints
{
    // TODO: rename and handle them correctly
    const string GetPublisherEndpointName = "GetPublisherById";
    const string PublishersTag = "Publishers";

    public static void MapPublishersEndpoints(this WebApplication app)
    {

        var group = app.MapGroup("/publishers");

        // GET /publishers
        group.MapGet("/", async ([AsParameters] PaginationDto pagination, [FromServices] IPublisherService publisherService) =>
        {
            var result = await publisherService.GetPublishersAsync(pagination);
            return Results.Ok(result);
        })
        .WithSummary("Get Publishers")
        .WithDescription("Returns paginated list of publishers.")
        .Produces<PagedResult<PublisherDetailsDto>>(StatusCodes.Status200OK)
        .WithTags(PublishersTag);

        // GET /publishers/1
        group.MapGet("/{id}", async (int id, [FromServices] IPublisherService publisherService) =>
        {
            var publisher = await publisherService.GetByIdAsync(id);

            return publisher is null ? Results.NotFound() : Results.Ok(publisher);
        })
           .WithName(GetPublisherEndpointName)
           .WithSummary("Get publisher by ID")
           .WithDescription("Returns a single publisher based on their unique identifier. Returns 404 if the publisher doesn't exist.")
           .Produces<PublisherDetailsDto>(StatusCodes.Status200OK)
           .Produces(StatusCodes.Status404NotFound)
           .WithTags(PublishersTag);

        // POST /publishers
        group.MapPost("/", async (CreatePublisherDto newPublisher, [FromServices] IPublisherService publisherService) =>
        {
            var createdPublisher = await publisherService.CreateAsync(newPublisher);

            return Results.CreatedAtRoute(GetPublisherEndpointName, new { id = createdPublisher.Id }, createdPublisher);
        })
            .WithName("CreatePublisher")
            .WithSummary("Create publisher")
            .WithDescription("Creates a publisher object and returns it with location.")
            .Produces<PublisherDetailsDto>(StatusCodes.Status201Created)
            .WithTags(PublishersTag);

        // PUT /publishers/1
        group.MapPut("/{id}", async (int id, UpdatePublisherDto updatedPublisher, [FromServices] IPublisherService publisherService) =>
        {
            var existingPublisher = await publisherService.UpdateAsync(id, updatedPublisher);

            return existingPublisher ? Results.NoContent() : Results.NotFound();
        })
        .WithSummary("Update publisher")
        .WithDescription("Updated a publisher basedon their unique identifier.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags(PublishersTag);

        // DELETE /publishers/1
        group.MapDelete("/{id}", async (int id, [FromServices] IPublisherService publisherService) =>
        {
            var deletedPublisher = await publisherService.DeleteAsync(id);

            return deletedPublisher ? Results.NoContent() : Results.NotFound();
        })
        .WithSummary("Delete publisher")
        .WithDescription("Deletes a publisher basedon their unique identifier.")
        .Produces(StatusCodes.Status204NoContent)
        .Produces(StatusCodes.Status404NotFound)
        .WithTags(PublishersTag);
    }

    // TODO:
    // get single publisher games
    // update single publisher games
    // remove single publisher games

}
