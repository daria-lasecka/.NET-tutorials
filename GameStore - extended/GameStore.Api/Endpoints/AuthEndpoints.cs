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
            IConfiguration configuration) =>
        {
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return Results.BadRequest(result.Errors);

            // Assign default role (User) to new user
            await userManager.AddToRoleAsync(user, Authorization.Roles.User.ToString());

            // Generate JWT token with roles
            var token = await GenerateJwtTokenAsync(user, userManager, configuration);

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

            // Generate JWT token (with roles)
            var token = await GenerateJwtTokenAsync(user, userManager, configuration);

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

    // Method to generate the JWT token with role claims
    private static async Task<string> GenerateJwtTokenAsync(User user, UserManager<User> userManager, IConfiguration configuration)
    {
        var issuer = configuration["JWT:Issuer"];
        var audience = configuration["JWT:Audience"];

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
        };

        // Add role claims (required for authorization policies to work)
        var roles = await userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT:Key"] ?? string.Empty));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Get duration from configuration
        double durationInMinutes = 60;
        var durationStr = configuration["JWT:DurationInMinutes"];
        if (!string.IsNullOrEmpty(durationStr) && double.TryParse(durationStr, out var d))
            durationInMinutes = d;

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(durationInMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}