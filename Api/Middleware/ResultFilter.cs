using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using VanBora.Domain.Common;

namespace Api.Middleware;

/// <summary>
/// Action filter that intercepts <see cref="IAppResult"/> return values from controllers
/// and converts them to proper HTTP responses — no reflection needed.
/// </summary>
public class ResultFilter : IAsyncResultFilter
{
    private readonly ILogger<ResultFilter> _logger;

    public ResultFilter(ILogger<ResultFilter> logger)
    {
        _logger = logger;
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            var valueType = objectResult.Value?.GetType().FullName ?? "(null)";
            _logger.LogInformation("ResultFilter: action={Action}, valueType={ValueType}, isIAppResult={IsIApp}",
                context.ActionDescriptor.DisplayName,
                valueType,
                objectResult.Value is IAppResult);

            if (objectResult.Value is IAppResult result)
            {
                _logger.LogInformation("ResultFilter: Intercepted IAppResult. IsSuccess={IsSuccess}, HasValue={HasValue}, ErrorType={ErrorType}",
                    result.IsSuccess, result.HasValue, result.IsFailure ? result.Error.Type.ToString() : "N/A");

                if (result.IsFailure)
                {
                    var errorResult = MapErrorToActionResult(result.Error);
                    context.Result = errorResult;
                    return;
                }

                // Success
                context.Result = result.HasValue
                    ? new OkObjectResult(result.GetValue())
                    : new NoContentResult();

                await next();
                return;
            }
        }
        else
        {
            _logger.LogInformation("ResultFilter: Not an ObjectResult. Type={ResultType}", context.Result?.GetType().FullName);
        }

        await next();
    }

    private static IActionResult MapErrorToActionResult(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.Validation => (int)HttpStatusCode.BadRequest,
            ErrorType.NotFound => (int)HttpStatusCode.NotFound,
            ErrorType.Conflict => (int)HttpStatusCode.Conflict,
            ErrorType.Unauthorized => (int)HttpStatusCode.Unauthorized,
            ErrorType.Forbidden => (int)HttpStatusCode.Forbidden,
            ErrorType.Failure => (int)HttpStatusCode.InternalServerError,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var response = new
        {
            type = GetErrorTypeUri(error.Type),
            title = GetErrorTitle(error.Type),
            status = statusCode,
            error = new
            {
                code = error.Code,
                message = error.Message
            },
            traceId = string.Empty
        };

        return new ObjectResult(response)
        {
            StatusCode = statusCode
        };
    }

    private static string GetErrorTypeUri(ErrorType type) => type switch
    {
        ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        ErrorType.Unauthorized => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        ErrorType.Forbidden => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        ErrorType.Failure => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
    };

    private static string GetErrorTitle(ErrorType type) => type switch
    {
        ErrorType.Validation => "One or more validation errors occurred.",
        ErrorType.NotFound => "The requested resource was not found.",
        ErrorType.Conflict => "The request conflicts with the current state of the resource.",
        ErrorType.Unauthorized => "Authentication is required.",
        ErrorType.Forbidden => "You do not have permission to perform this action.",
        ErrorType.Failure => "An internal server error occurred.",
        _ => "An unexpected error occurred."
    };
}
