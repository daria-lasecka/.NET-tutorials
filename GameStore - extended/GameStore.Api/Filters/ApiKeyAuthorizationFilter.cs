using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class ApiKeyAuthorizationFilter(IConfiguration configuration) : IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var apiKey = context.HttpContext.Request.Headers["X-Api-Key"].FirstOrDefault();
        var expectedKey = configuration["ApiSettings:ApiKey"];

        if (string.IsNullOrEmpty(apiKey) || apiKey != expectedKey)
        {
            context.Result = new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Invalid API Key",
                Detail = "Provide a valid API key in the X-Api-Key header."
            });
        }
    }
}