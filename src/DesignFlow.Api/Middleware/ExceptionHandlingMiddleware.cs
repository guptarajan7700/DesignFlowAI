using System.Net;
using System.Text.Json;
using DesignFlow.Application.Common;
using FluentValidation;

namespace DesignFlow.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await WriteProblemAsync(context, exception);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (status, title) = exception switch
        {
            UnauthorizedAppException => (HttpStatusCode.Unauthorized, "Unauthorized"),
            ForbiddenException => (HttpStatusCode.Forbidden, "Forbidden"),
            NotFoundException => (HttpStatusCode.NotFound, "Not found"),
            ConflictException => (HttpStatusCode.Conflict, "Conflict"),
            AppValidationException => (HttpStatusCode.BadRequest, "Validation failed"),
            ValidationException => (HttpStatusCode.BadRequest, "Validation failed"),
            _ => (HttpStatusCode.InternalServerError, "Server error")
        };

        if (status == HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogInformation(exception, "Request failed with {StatusCode}", (int)status);
        }

        var detail = status == HttpStatusCode.InternalServerError && !_environment.IsDevelopment()
            ? "An unexpected error occurred."
            : exception.Message;

        context.Response.StatusCode = (int)status;
        context.Response.ContentType = "application/problem+json";
        var problem = new
        {
            type = $"https://httpstatuses.com/{(int)status}",
            title,
            status = (int)status,
            detail
        };
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, JsonOptions));
    }
}
