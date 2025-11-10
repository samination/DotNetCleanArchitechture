using Domain.Helpers.Exceptions;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Middleware
{
    /// <summary>
    /// Custom Error Handler Middlware for requests.<br />
    /// If an exception is thrown, this middleware will catch it and return a custom response based on the status code defined in the switch statement.<br />
    /// If the exception is not handled, it will be logged and a 500 status code will be returned.<br />
    /// This error handler will automatically log the error message provided in the exception message to the console. No need to do that in the service.
    /// </summary>
    internal class ErrorHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
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
            catch (Exception error)
            {
                var response = context.Response;
                response.ContentType = "application/problem+json";
                Guid id = Guid.NewGuid(); // Generate new id for this error
                string errorType = error.GetType().Name;
                string humanizedErrorType = errorType.Humanize();
                string message = string.IsNullOrWhiteSpace(error.Message)
                    ? $"An unexpected {humanizedErrorType.ToLower()} occurred."
                    : error.Message;
                int statusCode;
                string title;

                const string WarningTemplate = "{ErrorSummary} ID: {ErrorId} - Message: {ErrorMessage}";

                switch (error)
                {
                    case ConcurrencyException:
                        _logger.LogWarning(
                            WarningTemplate,
                            "A concurrency conflict occurred.",
                            id,
                            message);
                        statusCode = StatusCodes.Status409Conflict;
                        title = "Concurrency conflict";
                        break;
                    // Custom Exception - located in Core.Domain.Helpers.Exceptions
                    // You can add as many custom exceptions you would like to handle
                    // Remember to add them to the switch statement with a status code for the response
                    case CustomException:
                        // custom application error
                        _logger.LogError(
                            "A {ErrorType} occurred. ID: {ErrorId} - Message: {ErrorMessage}",
                            humanizedErrorType,
                            id,
                            message);
                        statusCode = StatusCodes.Status400BadRequest;
                        title = humanizedErrorType;
                        break;
                    case KeyNotFoundException:
                        // not found error
                        var resourceSummary = $"Resource not found ({humanizedErrorType})";
                        _logger.LogWarning(
                            WarningTemplate,
                            resourceSummary,
                            id,
                            message);
                        statusCode = StatusCodes.Status404NotFound;
                        title = "Resource not found";
                        break;
                    default:
                        // unhandled error
                        var loweredErrorType = humanizedErrorType.ToLowerInvariant();
                        _logger.LogError(
                            error,
                            "An unexpected {ErrorType} occurred. ID: {ErrorId} - Message: {ErrorMessage}",
                            loweredErrorType,
                            id,
                            message);
                        message = $"An unexpected {loweredErrorType} occurred. Please try again later.";
                        statusCode = StatusCodes.Status500InternalServerError;
                        title = "Unexpected error";
                        break;
                }

                response.StatusCode = statusCode;

                var problemDetails = new ProblemDetails
                {
                    Status = statusCode,
                    Title = title,
                    Detail = message,
                    Instance = context.Request.Path,
                    Type = statusCode switch
                    {
                        StatusCodes.Status400BadRequest => "https://httpstatuses.io/400",
                        StatusCodes.Status404NotFound => "https://httpstatuses.io/404",
                        StatusCodes.Status409Conflict => "https://httpstatuses.io/409",
                        _ => "https://httpstatuses.io/500"
                    }
                };

                problemDetails.Extensions["errorId"] = id.ToString();
                problemDetails.Extensions["traceId"] = context.TraceIdentifier;

                await response.WriteAsJsonAsync(problemDetails);
            }
        }
    }
}
