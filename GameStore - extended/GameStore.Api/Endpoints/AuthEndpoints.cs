using GameStore.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        // Registration endpoint
        group.MapPost("/register", async (
            UserManager<User> userManager,
            RegisterDto request,
            IConfiguration configuration) => // Inject IConfiguration here
        {
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return Results.BadRequest(result.Errors);

            // After registration, log in the user automatically and return a JWT token
            var token = GenerateJwtToken(user, request.Password, configuration); // Pass IConfiguration here

            return Results.Ok(new { Token = token });
        });

        // Login endpoint
        group.MapPost("/login", async (
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IConfiguration configuration,
            LoginDto request) =>
        {
            // Find the user by email
            var user = await userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Results.Unauthorized();

            // Check the password
            var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Results.Unauthorized();

            // Generate JWT token
            var token = GenerateJwtToken(user, request.Password, configuration);

            return Results.Ok(new { Token = token });
        });

        // Logout endpoint
        // JWT doesn't need to be logged out server-side; we'll just send a logout message.
        group.MapPost("/logout", async () =>
        {
            // JWT simply expires, so there’s nothing to do for logout on the server side
            return Results.Ok("User logged out");
        });

        // "Me" endpoint - Returns user info from JWT token
        group.MapGet("/me", (HttpContext ctx) =>
        {
            var user = ctx.User;

            if (!user.Identity?.IsAuthenticated ?? true)
                return Results.Unauthorized();

            return Results.Ok(new
            {
                Name = user.Identity?.Name,
                Claims = user.Claims.Select(c => new { c.Type, c.Value })
            });
        }).RequireAuthorization();
    }

    // Method to generate the JWT token
    private static string GenerateJwtToken(User user, string password, IConfiguration configuration)
    {
        var issuer = configuration["JWT:Issuer"];
        var audience = configuration["JWT:Audience"];
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iss, issuer),
            new Claim(JwtRegisteredClaimNames.Aud, audience),
            new Claim("id", user.Id), // You can add custom claims here, like user ID or roles
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Get the duration from appsettings.json
        int durationInMinutes = int.Parse(configuration["JWT:DurationInMinutes"]); // Default to 60 minutes

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddMinutes(durationInMinutes), // Set expiration time using configuration value
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}