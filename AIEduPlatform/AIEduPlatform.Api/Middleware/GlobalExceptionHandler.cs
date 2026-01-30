using AIEduPlatform.Application.Common.Exceptions;
using AIEduPlatform.Application.Common.Models;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace AIEduPlatform.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "An error occurred: {Message}", exception.Message);

            var (statusCode, title, errors) = exception switch
            {
                ValidationException validationException => HandleValidationException(validationException),
                NotFoundException => (StatusCodes.Status404NotFound, "Not Found", null),
                BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request", null),
                ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden", null),
                UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized", null),
                ArgumentException => (StatusCodes.Status400BadRequest, "Bad Request", null),
                KeyNotFoundException => (StatusCodes.Status404NotFound, "Not Found", null),
                UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Unauthorized", null),
                _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", null)
            };

            var errorResponse = new ErrorResponse
            {
                Title = title,
                Status = statusCode,
                Detail = exception.Message,
                Errors = errors
            };

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = "application/json";

            await httpContext.Response.WriteAsJsonAsync(errorResponse, cancellationToken);

            return true;
        }

        private static (int StatusCode, string Title, Dictionary<string, string[]>? Errors) HandleValidationException(
            ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );

            return (StatusCodes.Status400BadRequest, "Validation Error", errors);
        }
    }
}
