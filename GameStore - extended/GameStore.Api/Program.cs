using GameStore.Api.Data;
using GameStore.Api.Endpoints;
using GameStoreApi.Handlers;
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

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddValidation();

builder.AddGameStoreDb();

builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IPublisherService, PublisherService>();
builder.Services.AddScoped<IUserService, UserService>();

// Bind JWT settings to the strongly-typed JWT class so IOptions<JWT> can be injected
builder.Services.Configure<JWT>(builder.Configuration.GetSection("JWT"));

builder.Services.AddOpenApi();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["JWT:Issuer"],
            ValidAudience = builder.Configuration["JWT:Audience"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:Key"])
            )
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly",
        policy => policy.RequireRole("Administrator"));
    options.AddPolicy("AdminOrModerator",
        policy => policy.RequireRole("Administrator", "Moderator"));
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseExceptionHandler();

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

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.MigrateDbAndSeedAsync();
}


app.Run();

public partial class Program { }

// during the course instead of running 
//  $env:ConnectionStrings__GameStore="Data Source=Production.db" (Windows's Power Shell)
// run
//  export ConnectionStrings__GameStore="Data Source=Production.db"

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

/*
Header: Authorization Bearer pasted_token
Examples ready to copy (with random genres):
Publishers:
{
    "name": "Team Cherry"
} 

{
    "name": "Supergiant Games"
}

Games:
{
    "name": "Hollow Knight",
    "publisherId": Check publisher ID (Team Cherry)
    "genreIds": [
        4,
        5
    ],
    "price": 14.99,
    "releaseDate": "1.1.1990" // "2017-02-24" <- correct date and format, added wrong one to show exception
}

{
    "name": "Hollow Knight: Silksong",
    "publisherId":  Check publisher ID (Team Cherry)
    "genreIds": [
        2,
        3,
        5
    ],
    "price": 19.99,
    "releaseDate": "2025-09-04"
}

{
    "name": "Hades",
    "publisherId": Check publisher ID (Supergiant Games)
    "genreIds": [
        1,
        2,
        5
    ],
    "price": 24.99,
    "releaseDate": "2020-09-17"
}

{
    "name": "The Legend of Zelda: Tears of the Kingdom",
    "publisherId": Check publisher ID (Nintendo)
    "genreIds": [
        4,
        5
    ],
    "price": 69.99,
    "releaseDate": "2023-05-12"
}

*/