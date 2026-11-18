
using DalilAcademy.Core.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DalilAcademy.API.Middleware
{
    public sealed class GlobalExceptionHandler(
        IProblemDetailsService problemDetailsService,
        ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(
            HttpContext context,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, title) = exception switch
            {
                ValidationException =>
                    (StatusCodes.Status400BadRequest, "Validation failed"),

                NotFoundException =>
                    (StatusCodes.Status404NotFound, "Resource not found"),

                ConflictException =>
                    (StatusCodes.Status409Conflict, "Conflict"),

                UnauthorizedException =>
                    (StatusCodes.Status401Unauthorized, "Unauthorized"),

                InvalidFileException => 
                    (StatusCodes.Status400BadRequest, "Invalid file"),
                _ =>
                    (StatusCodes.Status500InternalServerError,
                     "An unexpected error occurred")
            };

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "An unexpected error occurred.");
            }

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = exception is
                    NotFoundException or
                    ConflictException or
                    UnauthorizedException or
                    InvalidCastException or
                    InvalidFileException
                        ? exception.Message
                        : null
            };

            if (exception is ValidationException validationException)
            {
                problemDetails.Extensions["errors"] =
                    validationException.Errors
                        .GroupBy(error => error.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(error => error.ErrorMessage).ToArray()
                        );
            }

            context.Response.StatusCode = statusCode;

            await problemDetailsService.TryWriteAsync(
                new ProblemDetailsContext
                {
                    HttpContext = context,
                    ProblemDetails = problemDetails
                });

            return true;
        }
    }
}
