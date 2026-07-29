using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using GameStore.Api.Common;
using GameStore.Api.Dtos;
using GameStore.Tests.Infrastructure;

namespace GameStore.Tests.Endpoints;

public class GamesEndpointsTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public GamesEndpointsTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    private void AsAdmin()
    {
        TestAuthHandler.Role = "Administrator";
    }

    private void AsModerator()
    {
        TestAuthHandler.Role = "Moderator";
    }

    private void AsAnonymous()
    {
        TestAuthHandler.Role = null;
    }
    
    private async Task<int> GetExistingGameId()
    {
        var response = await _client.GetAsync(
            "/games?pageNumber=1&pageSize=10");

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<PagedResult<GameSummaryDto>>();

        return result!.Items.First().Id;
    }


    // -----------------------------
    // GET /games
    // -----------------------------

    [Fact]
    public async Task GetGames_ReturnsOk_ForAnonymousUser()
    {
        AsAnonymous();

        var response = await _client.GetAsync(
            "/games?pageNumber=1&pageSize=10");

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);
    }


    // -----------------------------
    // GET /games/{id}
    // -----------------------------

    [Fact]
    public async Task GetGame_ReturnsGame_WhenExists()
    {
        AsAnonymous();

        var response = await _client.GetAsync("/games/1");

        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);
    }


    [Fact]
    public async Task GetGame_ReturnsNotFound_WhenMissing()
    {
        AsAnonymous();

        var response = await _client.GetAsync("/games/999");

        response.StatusCode.Should()
            .Be(HttpStatusCode.NotFound);
    }


    // -----------------------------
    // POST /games
    // Requires AdminOrModerator
    // -----------------------------

    [Fact]
    public async Task CreateGame_AllowsAdministrator()
    {
        AsAdmin();

        var request = new CreateGameDto(
            "Celeste",
            1,
            [],
            19.99m,
            new DateOnly(2018, 1, 25));

        var response = await _client.PostAsJsonAsync(
            "/games",
            request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Created);
    }


    [Fact]
    public async Task CreateGame_AllowsModerator()
    {
        AsModerator();

        var request = new CreateGameDto(
            "Dead Cells",
            1,
            [],
            24.99m,
            new DateOnly(2018, 8, 7));

        var response = await _client.PostAsJsonAsync(
            "/games",
            request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Created);
    }


    [Fact]
    public async Task CreateGame_ReturnsUnauthorized_WhenAnonymous()
    {
        AsAnonymous();

        var request = new CreateGameDto(
            "Celeste",
            1,
            [],
            19.99m,
            new DateOnly(2018, 1, 25));

        var response = await _client.PostAsJsonAsync(
            "/games",
            request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // -----------------------------
    // PUT /games/{id}
    // Requires AdminOrModerator
    // -----------------------------

    [Fact]
    public async Task UpdateGame_AllowsAdministrator()
    {
        AsAdmin();

        var request = new UpdateGameDto(
            "Hades Updated",
            1,
            [],
            39.99m,
            new DateOnly(2020, 9, 17));

        var response = await _client.PutAsJsonAsync(
            "/games/1",
            request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.NoContent);
    }


    [Fact]
    public async Task UpdateGame_AllowsModerator()
    {
        AsModerator();

        var gameId = await GetExistingGameId();

        var request = new UpdateGameDto(
            "Hades Updated",
            1,
            [],
            39.99m,
            new DateOnly(2020, 9, 17));

        var response = await _client.PutAsJsonAsync(
            $"/games/{gameId}",
            request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task UpdateGame_ReturnsUnauthorized_WhenAnonymous()
    {
        AsAnonymous();

        var request = new UpdateGameDto(
            "Hades Updated",
            1,
            [],
            39.99m,
            new DateOnly(2020, 9, 17));

        var response = await _client.PutAsJsonAsync(
            "/games/1",
            request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }


    // -----------------------------
    // DELETE /games/{id}
    // Requires AdminOnly
    // -----------------------------

    [Fact]
    public async Task DeleteGame_AllowsAdministrator()
    {
        AsAdmin();

        var response = await _client.DeleteAsync("/games/1");

        response.StatusCode.Should()
            .Be(HttpStatusCode.NoContent);
    }


    [Fact]
    public async Task DeleteGame_ReturnsForbidden_ForModerator()
    {
        AsModerator();

        var response = await _client.DeleteAsync("/games/1");

        response.StatusCode.Should()
            .Be(HttpStatusCode.Forbidden);
    }


    [Fact]
    public async Task DeleteGame_ReturnsUnauthorized_WhenAnonymous()
    {
        AsAnonymous();

        var response = await _client.DeleteAsync("/games/1");

        response.StatusCode.Should()
            .Be(HttpStatusCode.Unauthorized);
    }
    
    [Fact]
    public async Task CreateGame_ReturnsBadRequest_WhenDateFormatIsInvalid()
    {
        AsAdmin();

        var request = new
        {
            name = "Celeste",
            publisherId = 1,
            genreIds = Array.Empty<int>(),
            price = 19.99,
            releaseDate = "17/09/2020"
        };

        var response = await _client.PostAsJsonAsync(
            "/games",
            request);

        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }
}