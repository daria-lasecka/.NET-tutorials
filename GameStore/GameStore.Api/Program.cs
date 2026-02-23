using GameStore.Api.Data;
using GameStore.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.AddGameStoreDb();

var app = builder.Build();

app.MapGamesEndpoints();
app.MapGenresEndpoints();

app.MigrateDb();

app.Run();

// during the course instead of running 
//  $env:ConnectionStrings__GameStore="Data Source=Production.db" (Windows's Power Shell)
// run
//  export ConnectionStrings__GameStore="Data Source=Production.db"
