using GameStore.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Tests.Infrastructure;

public static class DbContextFactory
{
    public static GameStoreContext Create(Action<GameStoreContext>? seed = null)
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<GameStoreContext>()
            .UseSqlite(connection)
            .Options;

        var context = new GameStoreContext(options);

        context.Database.EnsureCreated();

        seed?.Invoke(context);
        context.SaveChanges();

        return context;
    }
}