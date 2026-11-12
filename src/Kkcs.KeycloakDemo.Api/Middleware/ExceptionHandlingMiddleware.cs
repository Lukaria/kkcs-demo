using FluentValidation;
using Kkcs.KeycloakDemo.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Kkcs.KeycloakDemo.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            var errors = exception.Errors
                .GroupBy(failure => failure.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(failure => failure.ErrorMessage).ToArray());

            await WriteProblem(context, StatusCodes.Status400BadRequest, "Validation failed", errors);
        }
        catch (KeycloakRequestException exception)
        {
            var statusCode = (int)exception.StatusCode;
            if (statusCode < 400 || statusCode > 499)
            {
                statusCode = StatusCodes.Status502BadGateway;
            }

            logger.LogWarning(exception, "Keycloak rejected a request");
            await WriteProblem(context, statusCode, "Keycloak request failed", detail: exception.Message);
        }
        catch (UnauthorizedAccessException exception)
        {
            await WriteProblem(context, StatusCodes.Status401Unauthorized, "Unauthorized", detail: exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An unhandled exception occurred");
            await WriteProblem(context, StatusCodes.Status500InternalServerError, "Internal server error");
        }
    }

    private static async Task WriteProblem(
        HttpContext context,
        int statusCode,
        string title,
        IDictionary<string, string[]>? errors = null,
        string? detail = null)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = statusCode;
        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };
        problem.Extensions["traceId"] = context.TraceIdentifier;

        if (errors is not null)
        {
            problem.Extensions["errors"] = errors;
        }

        await context.Response.WriteAsJsonAsync(problem, context.RequestAborted);
    }
}
