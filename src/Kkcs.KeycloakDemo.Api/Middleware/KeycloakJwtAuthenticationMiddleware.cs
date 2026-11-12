using System.Net.Http.Headers;
using Kkcs.KeycloakDemo.Infrastructure.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace Kkcs.KeycloakDemo.Api.Middleware;

public sealed class KeycloakJwtAuthenticationMiddleware(
    RequestDelegate next,
    ILogger<KeycloakJwtAuthenticationMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context, KeycloakJwtTokenValidator tokenValidator)
    {
        var authorization = context.Request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(authorization))
        {
            await next(context);
            return;
        }

        if (!AuthenticationHeaderValue.TryParse(authorization, out var header)
            || !string.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrWhiteSpace(header.Parameter))
        {
            await WriteUnauthorized(context, "invalid_token", "A valid Bearer access token is required.");
            return;
        }

        try
        {
            context.User = await tokenValidator.ValidateAsync(header.Parameter, context.RequestAborted);
            await next(context);
        }
        catch (SecurityTokenException exception)
        {
            logger.LogWarning(exception, "JWT authentication failed");
            await WriteUnauthorized(context, "invalid_token", "The access token is invalid or expired.");
        }
    }

    private static async Task WriteUnauthorized(HttpContext context, string error, string description)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.Headers.WWWAuthenticate = $"Bearer realm=\"kkcs-api\", error=\"{error}\", error_description=\"{description}\"";

        await context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Authentication failed",
            Detail = description
        }, context.RequestAborted);
    }
}
