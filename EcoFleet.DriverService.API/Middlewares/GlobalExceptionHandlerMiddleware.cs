using System.Net;
using System.Text.Json;
using EcoFleet.BuildingBlocks.Application.Exceptions;
using EcoFleet.BuildingBlocks.Domain.Exceptions;

namespace EcoFleet.DriverService.API.Middlewares;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
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
        catch (ValidationErrorException ex)
        {
            _logger.LogWarning("Validation error on {Path}: {@Errors}", context.Request.Path, ex.Errors);
            await HandleExceptionAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Resource not found on {Path}: {Message}", context.Request.Path, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning("Business rule violation on {Path}: {Message}", context.Request.Path, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning("Domain rule violation on {Path}: {Message}", context.Request.Path, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing request {Path}", context.Request.Path);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        HttpStatusCode statusCode;
        object response;

        switch (exception)
        {
            case ValidationErrorException validationEx:
                // 400 Bad Request - Return the dictionary of validation errors
                statusCode = HttpStatusCode.BadRequest;
                response = new
                {
                    Type = "ValidationError",
                    Errors = validationEx.Errors
                };
                break;

            case NotFoundException notFoundEx:
                // 404
                statusCode = HttpStatusCode.NotFound;
                response = new
                {
                    Type = "NotFound",
                    Message = notFoundEx.Message
                };
                break;

            case BusinessRuleException businessEx:
                // 409 Conflict - The request conflicts with the current state of a resource
                statusCode = HttpStatusCode.Conflict;
                response = new
                {
                    Type = "BusinessRuleViolation",
                    Message = businessEx.Message
                };
                break;

            case DomainException domainEx:
                // 422 Unprocessable Entity - Domain invariant violation
                statusCode = HttpStatusCode.UnprocessableEntity;
                response = new
                {
                    Type = "DomainError",
                    Message = domainEx.Message
                };
                break;

            default:
                // 500 Internal Server Error - Hide implementation details
                statusCode = HttpStatusCode.InternalServerError;
                response = new
                {
                    Type = "ServerError",
                    Message = "An unexpected error occurred."
                };
                break;
        }

        context.Response.StatusCode = (int)statusCode;
        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
