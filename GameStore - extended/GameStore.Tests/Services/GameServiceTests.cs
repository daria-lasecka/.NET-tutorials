using FluentAssertions;
using GameStore.Api.Data;
using GameStore.Api.Dtos;
using GameStore.Api.Models;
using GameStore.Tests.Infrastructure;

namespace GameStore.Tests.Services;

public class GameServiceTests
{
    [Fact]
    public async Task GetByIdAsync_ShouldReturnGame_WhenGameExists()
    {
        using var db = DbContextFactory.Create();

        var publisher = new Publisher { Name = "Supergiant Games" };
        db.Publishers.Add(publisher);
        await db.SaveChangesAsync();

        var game = new Game
        {
            Name = "Hades",
            PublisherId = publisher.Id,
            Price = 69.99m,
            ReleaseDate = new DateOnly(2020, 09, 17)
        };

        db.Games.Add(game);
        await db.SaveChangesAsync();

        var service = new GameService(db);

        var result = await service.GetByIdAsync(game.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Hades");
        result.PublisherId.Should().Be(publisher.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenGameDoesNotExist()
    {
        using var db = DbContextFactory.Create();

        var service = new GameService(db);

        var result = await service.GetByIdAsync(999);

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateGame()
    {
        using var db = DbContextFactory.Create();

        var publisher = new Publisher { Name = "Nintendo" };
        db.Publishers.Add(publisher);
        await db.SaveChangesAsync();

        var dto = new CreateGameDto(
            "Super Mario Odyssey",
            publisher.Id,
            [],
            59.99m,
            new DateOnly(2017, 10, 27));

        var service = new GameService(db);

        var result = await service.CreateAsync(dto);

        db.Games.Should().ContainSingle();

        result.Name.Should().Be("Super Mario Odyssey");
        result.PublisherId.Should().Be(publisher.Id);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteGame_WhenGameExists()
    {
        using var db = DbContextFactory.Create();

        var publisher = new Publisher { Name = "Nintendo" };
        db.Publishers.Add(publisher);
        await db.SaveChangesAsync();

        var game = new Game
        {
            Name = "Super Mario Odyssey",
            PublisherId = publisher.Id
        };

        db.Games.Add(game);
        await db.SaveChangesAsync();

        var service = new GameService(db);

        var deleted = await service.DeleteAsync(game.Id);

        deleted.Should().BeTrue();
        db.Games.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenGameDoesNotExist()
    {
        using var db = DbContextFactory.Create();

        var service = new GameService(db);

        var deleted = await service.DeleteAsync(999);

        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateGame()
    {
        using var db = DbContextFactory.Create();

        var publisher = new Publisher { Name = "Supergiant Games" };
        db.Publishers.Add(publisher);
        await db.SaveChangesAsync();

        var game = new Game
        {
            Name = "Hades",
            PublisherId = publisher.Id,
            Price = 50,
            ReleaseDate = new DateOnly(2020, 09, 17)
        };

        db.Games.Add(game);
        await db.SaveChangesAsync();

        var dto = new UpdateGameDto(
            "Hades II",
            publisher.Id,
            [],
            70,
            new DateOnly(2025, 09, 25));

        var service = new GameService(db);

        var updated = await service.UpdateAsync(game.Id, dto);

        updated.Should().BeTrue();

        var updatedGame = await db.Games.FindAsync(game.Id);

        updatedGame.Should().NotBeNull();
        updatedGame!.Name.Should().Be("Hades II");
        updatedGame.Price.Should().Be(70);
        updatedGame.ReleaseDate.Should().Be(new DateOnly(2025, 09, 25));
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenGameDoesNotExist()
    {
        using var db = DbContextFactory.Create();

        var service = new GameService(db);

        var dto = new UpdateGameDto(
            "Hades",
            1,
            [],
            50,
            new DateOnly(2024, 1, 1));

        var result = await service.UpdateAsync(999, dto);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetGamesAsync_ShouldReturnAllGames()
    {
        using var db = DbContextFactory.Create();

        var publisher = new Publisher { Name = "Supergiant Games" };
        db.Publishers.Add(publisher);
        await db.SaveChangesAsync();

        db.Games.AddRange(
            new Game
            {
                Name = "Hades",
                PublisherId = publisher.Id,
                Price = 59.99m
            },
            new Game
            {
                Name = "Hades II",
                PublisherId = publisher.Id,
                Price = 49.99m
            });

        await db.SaveChangesAsync();

        var service = new GameService(db);

        var result = await service.GetGamesAsync(
            new GameFilterDto(),
            new PaginationDto());

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetGamesAsync_ShouldFilterByName()
    {
        using var db = DbContextFactory.Create();

        var supergiant = new Publisher
        {
            Name = "Supergiant Games"
        };

        var teamCherry = new Publisher
        {
            Name = "Team Cherry"
        };

        db.Publishers.AddRange(supergiant, teamCherry);
        await db.SaveChangesAsync();

        db.Games.AddRange(
            new Game
            {
                Name = "Hades",
                PublisherId = supergiant.Id
            },
            new Game
            {
                Name = "Hollow Knight",
                PublisherId = teamCherry.Id
            });

        await db.SaveChangesAsync();

        var service = new GameService(db);

        var result = await service.GetGamesAsync(
            new GameFilterDto
            {
                Name = "Hades"
            },
            new PaginationDto());

        result.Items.Should().ContainSingle();

        result.Items.Should()
            .Contain(g => g.Name == "Hades");
    }

    [Fact]
    public async Task GetGamesAsync_ShouldReturnCorrectPage()
    {
        using var db = DbContextFactory.Create();

        var publisher = new Publisher { Name = "Supergiant Games" };
        db.Publishers.Add(publisher);
        await db.SaveChangesAsync();

        for (int i = 1; i <= 25; i++)
        {
            db.Games.Add(new Game
            {
                Name = $"Game {i}",
                PublisherId = publisher.Id,
                Price = i
            });
        }

        await db.SaveChangesAsync();

        var service = new GameService(db);

        var result = await service.GetGamesAsync(
            new GameFilterDto(),
            new PaginationDto
            {
                PageNumber = 2,
                PageSize = 10
            });

        result.Items.Should().HaveCount(10);
        result.TotalCount.Should().Be(25);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(10);
    }
}