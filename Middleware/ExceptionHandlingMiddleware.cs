using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

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

            var problemDetails = new
            {
                type = $"https://httpstatuses.com/{statusCode}",
                title = statusCode == StatusCodes.Status400BadRequest
                    ? "The request is invalid."
                    : "An unexpected error occurred.",
                status = statusCode,
                traceId = context.TraceIdentifier
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, JsonSerializerOptions));
        }
    }
}
