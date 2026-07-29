using GameStore.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GameStore.Api.Data;

public static class DataExtensions
{

    public static async Task MigrateDbAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GameStoreContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var applicationDbContextSeed = scope.ServiceProvider.GetRequiredService<ApplicationDbContextSeed>();

        dbContext.Database.Migrate();

        await ApplicationDbContextSeed.SeedEssentialsAsync(userManager, roleManager);
    }
    // public static void MigrateDb(this WebApplication app)
    // {
    //     using var scope = app.Services.CreateScope();
    //     var DbContext = scope.ServiceProvider
    //                          .GetRequiredService<GameStoreContext>();
    //     DbContext.Database.Migrate();
    // }

    public static void AddGameStoreDb(this WebApplicationBuilder builder)
    {
        var connString = builder.Configuration.GetConnectionString("GameStore");

        // DbContext has a Scoped service lifetime because:
        // 1. It ensures that a new instance of DbContext is created per request
        // 2. DB connections are a limited ad expensive resource
        // 3. DbContext is not thread-safe. Scoped avoids concurrency issues
        // 4. Makes it easier to manage transactions and ensure data consistency
        // 5. Reusing a DbContext instance can lead to increaded memory usage

        builder.Services.AddSqlite<GameStoreContext>(
            connString,
            optionsAction: options => options.UseSeeding((context, _) =>
            {
                if (!context.Set<Genre>().Any())
                {
                    context.Set<Genre>().AddRange(
                        new Genre { Name = "Fighting" },
                        new Genre { Name = "RPG" },
                        new Genre { Name = "Platformer" },
                        new Genre { Name = "Racing" },
                        new Genre { Name = "Sports" }
                    );

                    context.SaveChanges();
                }
            })
        );

        // For JWT API - only add UserManager and RoleManager, not the full Identity with default authentication
        builder.Services.AddHttpContextAccessor();
        
        builder.Services.AddIdentityCore<User>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<GameStoreContext>()
                .AddDefaultTokenProviders();

        // Add SignInManager for password verification
        builder.Services.AddScoped<SignInManager<User>>();

        builder.Services.AddTransient<ApplicationDbContextSeed>();
    }
}
