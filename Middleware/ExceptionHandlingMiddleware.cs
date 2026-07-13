using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace Abysalto.StefanParch.Api.Middleware;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var statusCode = exception is ValidationException or ArgumentException
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status500InternalServerError;

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Unhandled exception while processing the request.");
            }
            else
            {
                logger.LogWarning(exception, "Request validation failed.");
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/problem+json";

            var problemDetails = new ProblemDetails
            {
                Type = $"https://httpstatuses.com/{statusCode}",
                Title = statusCode == StatusCodes.Status400BadRequest
                    ? "The request is invalid."
                    : "An unexpected error occurred.",
                Status = statusCode,
                Detail = statusCode == StatusCodes.Status400BadRequest ? exception.Message : null,
                Instance = context.Request.Path
            };

            problemDetails.Extensions["traceId"] = context.TraceIdentifier;

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonSerializerOptions));
        }
    }
}
