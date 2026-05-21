// Minimal working auth code example

// using System.IdentityModel.Tokens.Jwt;
// using System.Security.Claims;
// using System.Text;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;
// using Scalar.AspNetCore;
//
// var builder = WebApplication.CreateBuilder(args);
//
// // =====================
// // AUTHENTICATION (JWT)
// // =====================
// var jwtKey = "THIS_IS_DEMO_KEY_CHANGE_ME_123456789";
//
// builder.Services
//     .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters
//         {
//             ValidateIssuer = false,
//             ValidateAudience = false,
//             ValidateIssuerSigningKey = true,
//             IssuerSigningKey = new SymmetricSecurityKey(
//                 Encoding.UTF8.GetBytes(jwtKey))
//         };
//     });
//
// // =====================
// // AUTHORIZATION
// // =====================
// builder.Services.AddAuthorization(options =>
// {
//     options.AddPolicy("AdminOnly", policy =>
//         policy.RequireClaim(ClaimTypes.Role, "admin"));
// });
//
// // =====================
// // OPENAPI + SCALAR UI
// // =====================
// builder.Services.AddOpenApi();
//
// var app = builder.Build();
//
// app.UseAuthentication();
// app.UseAuthorization();
//
// // OpenAPI endpoint
// app.MapOpenApi();
//
// // Scalar UI (replaces Swagger UI)
// app.MapScalarApiReference(options =>
// {
//     options.Title = "OpenAPI + Scalar Demo";
// });
//
// // =====================
// // PUBLIC ENDPOINT
// // =====================
// app.MapGet("/", () => "Hello world (public)");
//
// // =====================
// // LOGIN (issues JWT)
// // =====================
// app.MapPost("/login", (LoginRequest login) =>
// {
//     if (login.Username != "admin" || login.Password != "password")
//         return Results.Unauthorized();
//
//     var claims = new[]
//     {
//         new Claim(ClaimTypes.Name, login.Username),
//         new Claim(ClaimTypes.Role, "admin")
//     };
//
//     var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
//     var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
//
//     var token = new JwtSecurityToken(
//         claims: claims,
//         expires: DateTime.UtcNow.AddHours(1),
//         signingCredentials: creds);
//
//     return Results.Ok(new
//     {
//         token = new JwtSecurityTokenHandler().WriteToken(token)
//     });
// });
//
// // =====================
// // AUTHENTICATED ENDPOINT
// // =====================
// app.MapGet("/secure", (ClaimsPrincipal user) =>
// {
//     return $"Hello {user.Identity?.Name}, you are authenticated!";
// })
// .RequireAuthorization();
//
// // =====================
// // ROLE-BASED ENDPOINT
// // =====================
// app.MapGet("/admin", () => "Admin content only")
// .RequireAuthorization("AdminOnly");
//
// app.Run();
//
// record LoginRequest(string Username, string Password);