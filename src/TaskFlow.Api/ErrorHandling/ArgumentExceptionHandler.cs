using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace TaskFlow.Api.ErrorHandling;

public sealed class ArgumentExceptionHandler(
    ILogger<ArgumentExceptionHandler> logger,
    IProblemDetailsService problemDetailsService) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ArgumentException)
            return false;

        logger.LogWarning("Invalid argument for {Method} {Path}: {Reason}",
            httpContext.Request.Method, httpContext.Request.Path.Value, exception.Message);

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid argument",
                Detail = exception.Message
            }
        });
    }
}