using System.Diagnostics;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Sentry;
using Shop.Application.Repositories;
using Shop.Domain.Entities;
using Shop.Domain.Exceptions;

namespace Shop.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    // IExceptionLogRepository is scoped, so it is resolved per-request via method
    // injection rather than through the singleton middleware's constructor.
    public async Task InvokeAsync(HttpContext context, IExceptionLogRepository exceptionLogRepository)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var statusCode = ResolveStatusCode(ex);

            // This middleware swallows the exception, so Sentry's own pipeline never sees it.
            // Only unexpected errors are reported; mapped domain errors (4xx) are expected.
            if (statusCode == HttpStatusCode.InternalServerError)
            {
                SentrySdk.CaptureException(ex);
                Activity.Current?.AddException(ex);
            }

            await LogExceptionAsync(context, ex, statusCode, exceptionLogRepository);
            await WriteProblemAsync(context, statusCode, ResolveDetail(ex, statusCode));
        }
    }

    private static HttpStatusCode ResolveStatusCode(Exception exception) => exception switch
    {
        EntityNotFoundException or BasketItemNotFoundException => HttpStatusCode.NotFound,
        DuplicateUserNameException
            or DuplicateEmailException
            or InsufficientStockException
            or DbUpdateConcurrencyException => HttpStatusCode.Conflict,
        AuthenticationFailedException or InvalidRefreshTokenException => HttpStatusCode.Unauthorized,
        InvalidPriceAmountException
            or InvalidAddressException
            or CurrencyMismatchException
            or InvalidQuantityException
            or DomainValidationException
            or ProductNotAvailableException
            or EmptyBasketException
            or InvalidOrderStateException
            or InvalidPaymentException => HttpStatusCode.BadRequest,
        _ => HttpStatusCode.InternalServerError
    };

    // Known domain exceptions carry a safe, user-facing message; anything else is
    // masked behind a generic message so internal details are not leaked.
    private static string ResolveDetail(Exception exception, HttpStatusCode statusCode) =>
        statusCode == HttpStatusCode.InternalServerError
            ? "An unexpected error occurred."
            : exception is DbUpdateConcurrencyException
                ? "The resource was modified by another request. Please retry."
                : exception.Message;

    private async Task LogExceptionAsync(
        HttpContext context,
        Exception exception,
        HttpStatusCode statusCode,
        IExceptionLogRepository repository)
    {
        try
        {
            var log = ExceptionLog.CreateByException(
                exception,
                (int)statusCode,
                context.Request.Path,
                context.Request.Method);

            await repository.AddAsync(log, context.RequestAborted);
        }
        catch (Exception loggingException)
        {
            // Never let a logging failure (e.g. Mongo unreachable) mask the original
            // error or break the response — record it to the console logger instead.
            logger.LogError(loggingException,
                "Failed to persist exception log to MongoDB for {ExceptionType}",
                exception.GetType().Name);
        }
    }

    private static Task WriteProblemAsync(HttpContext context, HttpStatusCode statusCode, string detail)
    {
        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = (int)statusCode;

        // Lets a caller quote the id of a failed request so its logs and trace can be found.
        var problem = new
        {
            status = (int)statusCode,
            title = statusCode.ToString(),
            detail,
            traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier
        };

        return context.Response.WriteAsJsonAsync(problem);
    }
}