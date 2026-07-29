using GameStore.Api.Data;
using GameStore.Api.Models;

namespace GameStore.Tests.Infrastructure;

public static class TestData
{
    public static void Seed(GameStoreContext db)
    {
        if (db.Games.Any())
        {
            return;
        }

        var supergiant = new Publisher
        {
            Name = "Supergiant Games"
        };

        var teamCherry = new Publisher
        {
            Name = "Team Cherry"
        };

        db.Publishers.AddRange(
            supergiant,
            teamCherry);

        db.Games.AddRange(
            new Game
            {
                Name = "Hades",
                Publisher = supergiant,
                Price = 29.99m,
                ReleaseDate = new DateOnly(2020, 9, 17)
            },
            new Game
            {
                Name = "Hollow Knight",
                Publisher = teamCherry,
                Price = 14.99m,
                ReleaseDate = new DateOnly(2017, 2, 24)
            });

        db.SaveChanges();
    }
}