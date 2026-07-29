using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GameStore.Tests.Infrastructure;

public class TestAuthHandler 
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public static string? Role { get; set; } = "Administrator";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (Role == null)
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var claims = new[]
        {
            new Claim(
                ClaimTypes.Name,
                "Test User"),

            new Claim(
                ClaimTypes.Role,
                Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            "Test");

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            "Test");

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}