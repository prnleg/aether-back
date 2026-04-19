using System.Text.Json;
using FluentValidation;

namespace Aether.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            await WriteErrorAsync(context, 422, "ValidationError", string.Join("; ", ex.Errors.Select(e => e.ErrorMessage)));
        }
        catch (UnauthorizedAccessException ex)
        {
            await WriteErrorAsync(context, 401, "Unauthorized", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            await WriteErrorAsync(context, 404, "NotFound", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteErrorAsync(context, 500, "InternalServerError", "An unexpected error occurred.");
        }
    }

    private static async Task WriteErrorAsync(HttpContext context, int statusCode, string error, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        var body = JsonSerializer.Serialize(new { error, message });
        await context.Response.WriteAsync(body);
    }
}
