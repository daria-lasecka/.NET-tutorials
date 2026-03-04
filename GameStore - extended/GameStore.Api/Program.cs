using GameStore.Api.Data;
using GameStore.Api.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.AddGameStoreDb();


builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGamesEndpoints();
app.MapGenresEndpoints();

app.MigrateDb();

// if (!app.Environment.IsDevelopment())
// {
//     app.UseHsts();
// }
// app.UseHttpsRedirection();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();               // Available at /openapi/v1.json
    app.MapScalarApiReference();    // Available at /scalar/v1
}

app.Run();

// during the course instead of running 
//  $env:ConnectionStrings__GameStore="Data Source=Production.db" (Windows's Power Shell)
// run
//  export ConnectionStrings__GameStore="Data Source=Production.db"
