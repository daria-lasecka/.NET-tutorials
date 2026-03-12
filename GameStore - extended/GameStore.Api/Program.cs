using GameStore.Api.Data;
using GameStore.Api.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.AddGameStoreDb();

builder.Services.AddScoped<IGameService, GameService>();

builder.Services.AddOpenApi();

var app = builder.Build();

// app.UseMiddleware<MaintenanceMiddleware>();
app.UseMaintenance();

app.MapGamesEndpoints();
app.MapGenresEndpoints();

app.MigrateDb();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // Available at /openapi/v1.json
    app.MapScalarApiReference();    // Available at /scalar/v1
    app.UseHttpsRedirection();
}
else
{
    app.UseHsts();
}

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseWhen(
    context => context.Request.Path.StartsWithSegments("/api"),
    appBuilder => appBuilder.UseMiddleware<RequestLoggingMiddleware>()
);

app.MapWhen(
    context => context.Request.Path.StartsWithSegments("/health"),
    appBuilder => appBuilder.Run(async context =>
    {
        await context.Response.WriteAsJsonAsync(new { Status = "Healthy" });
    })
);

/* 
Recommended execution order:

app.UseExceptionHandler();                   // 1. Catch all unhandled exceptions
app.UseHttpsRedirection();                   // 2. Redirect HTTP → HTTPS
app.UseRouting();                            // 3. Match routes
app.UseCors();                               // 4. CORS headers
app.UseAuthentication();                     // 5. Establish identity
app.UseAuthorization();                      // 6. Check permissions
app.UseOutputCache();                        // 7. Serve cached responses
app.UseRateLimiter();                        // 8. Enforce rate limits
app.UseResponseCompression();                // 9. Compress responses
app.UseMiddleware<RequestLoggingMiddleware>();// 10. Custom middleware
app.MapControllers();                        // 11. Execute endpoints

*/

app.Run();

// during the course instead of running 
//  $env:ConnectionStrings__GameStore="Data Source=Production.db" (Windows's Power Shell)
// run
//  export ConnectionStrings__GameStore="Data Source=Production.db"
