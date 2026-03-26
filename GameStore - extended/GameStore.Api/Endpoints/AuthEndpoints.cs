using GameStore.Api.Models;
using Microsoft.AspNetCore.Identity;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", async (
            UserManager<User> userManager,
            RegisterDto request) =>
        {
            var user = new User
            {
                UserName = request.Email,
                Email = request.Email
            };

            var result = await userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return Results.BadRequest(result.Errors);

            return Results.Ok("User created");
        });

        group.MapPost("/login", async (
            SignInManager<User> signInManager,
            LoginDto request) =>
        {
            var result = await signInManager.PasswordSignInAsync(
                request.Email,
                request.Password,
                true,
                false);

            if (!result.Succeeded)
                return Results.Unauthorized();

            return Results.Ok("Logged in");
        });

        group.MapPost("/logout", async (
            SignInManager<User> signInManager) =>
        {
            await signInManager.SignOutAsync();
            return Results.Ok("Logged out");
        });

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
}