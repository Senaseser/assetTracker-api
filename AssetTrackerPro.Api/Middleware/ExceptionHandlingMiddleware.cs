using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace AssetTrackerPro.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");

            var (statusCode, message) = ex switch
            {
                DbUpdateException => (HttpStatusCode.BadRequest, "Veritabanı güncellemesi başarısız."),
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "Beklenmeyen sunucu hatası.")
            };

            var payload = new ErrorResponse
            {
                Status = (int)statusCode,
                Message = message,
                TraceId = context.TraceIdentifier
            };

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = payload.Status;
            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
    }

    private sealed class ErrorResponse
    {
        public int Status { get; init; }
        public string Message { get; init; } = string.Empty;
        public string TraceId { get; init; } = string.Empty;
    }
}
