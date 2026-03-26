using GameStore.Api.Data;
using GameStore.Api.Endpoints;
using Microsoft.AspNetCore.Identity;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.AddGameStoreDb();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();

builder.Services.AddOpenApi();

builder.Services.AddProblemDetails();

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<GameStoreContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();

// app.UseMiddleware<MaintenanceMiddleware>();
app.UseMaintenance();

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

app.UseAuthentication();
app.UseAuthorization();

// app.UseMiddleware<RequestLoggingMiddleware>();
app.UseRequestLogging();
app.UseStatusCodePages();

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

app.MapGamesEndpoints();
app.MapGenresEndpoints();
app.MapPublishersEndpoints();
app.MapAuthEndpoints();

await app.MigrateDbAndSeedAsync();

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

/*
TODO: clean up Program.cs file using the below code as base...

using YourApi.Handlers;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
    };
});

// Register exception handlers (order matters for chaining)
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

app.MapOpenApi();
app.MapScalarApiReference(); // API docs at /scalar/v1

// Your endpoints here...

app.Run();

*/