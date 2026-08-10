using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Shop.Api.Contracts;

namespace Shop.Api.Middleware;

public sealed class ApiResultMiddleware(RequestDelegate next)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var originalBody = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // MVC may start the response while writing to the buffer. Registering this
        // up front ensures the eventual envelope is sent with compatible headers.
        context.Response.OnStarting(() =>
        {
            context.Response.ContentLength = null;
            context.Response.ContentType = "application/json; charset=utf-8";
            return Task.CompletedTask;
        });

        try
        {
            await next(context);

            context.Response.Body = originalBody;
            responseBody.Position = 0;

            if (MustNotHaveBody(context))
                return;

            if (context.Items.ContainsKey(typeof(ApiResult<>)) || context.Items.ContainsKey(typeof(ApiResult)))
            {
                await responseBody.CopyToAsync(originalBody, context.RequestAborted);
                return;
            }

            var payload = await ReadPayloadAsync(responseBody, context.RequestAborted);
            var status = context.Response.StatusCode;
            var result = status is >= StatusCodes.Status200OK and < StatusCodes.Status300MultipleChoices
                ? ApiResult<object?>.Succeeded(payload, status)
                : ApiResult<object?>.Failed(payload ?? GetDefaultError(status), status);

            await JsonSerializer.SerializeAsync(
                originalBody,
                result,
                SerializerOptions,
                context.RequestAborted);
        }
        finally
        {
            context.Response.Body = originalBody;
        }
    }

    private static bool MustNotHaveBody(HttpContext context) =>
        HttpMethods.IsHead(context.Request.Method)
        || context.Response.StatusCode is StatusCodes.Status204NoContent
            or StatusCodes.Status304NotModified;

    private static async Task<object?> ReadPayloadAsync(Stream responseBody, CancellationToken cancellationToken)
    {
        if (responseBody.Length == 0)
            return null;

        try
        {
            return await JsonSerializer.DeserializeAsync<JsonElement>(
                responseBody,
                SerializerOptions,
                cancellationToken);
        }
        catch (JsonException)
        {
            responseBody.Position = 0;
            using var reader = new StreamReader(responseBody, leaveOpen: true);
            return await reader.ReadToEndAsync(cancellationToken);
        }
    }

    private static object GetDefaultError(int status) => new
    {
        message = ReasonPhrases.GetReasonPhrase(status)
    };
}