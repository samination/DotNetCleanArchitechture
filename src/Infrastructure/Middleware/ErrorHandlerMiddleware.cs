using Domain.Helpers.Exceptions;
using Humanizer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

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
                response.ContentType = "application/json";
                Guid id = Guid.NewGuid(); // Generate new id for this error
                string errorType = error.GetType().Name;
                string humanizedErrorType = errorType.Humanize();
                string message = string.IsNullOrWhiteSpace(error.Message)
                    ? $"An unexpected {humanizedErrorType.ToLower()} occurred."
                    : error.Message;

                switch (error)
                {
                    // Custom Exception - located in Core.Domain.Helpers.Exceptions
                    // You can add as many custom exceptions you would like to handle
                    // Remember to add them to the switch statement with a status code for the response
                    case CustomException:
                        // custom application error
                        _logger.LogError($"A {humanizedErrorType} occurred. ID: {id} - Message: {message}");
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                    case KeyNotFoundException:
                        // not found error
                        _logger.LogWarning($"Resource not found ({humanizedErrorType}). ID: {id} - Message: {message}");
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                        break;
                    default:
                        // unhandled error
                        _logger.LogError(error, $"An unexpected {humanizedErrorType.ToLower()} occurred. ID: {id} - Message: {message}");
                        message = $"An unexpected {humanizedErrorType.ToLower()} occurred. Please try again later.";
                        response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        break;
                }

                var result = JsonSerializer.Serialize(new
                {
                    id = id.ToString(),
                    type = humanizedErrorType,
                    message,
                    statusCode = response.StatusCode,
                });
                await response.WriteAsync(result);
            }
        }
    }
}
