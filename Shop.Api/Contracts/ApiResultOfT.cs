using Microsoft.AspNetCore.Mvc;

namespace Shop.Api.Contracts;

public sealed class ApiResult<T>(
    T? data,
    bool success,
    int status,
    object? error = null,
    string? location = null
) : IActionResult
{
    public T? Data { get; } = data;
    public bool Success { get; } = success;
    public int Status { get; } = status;
    public object? Error { get; } = error;

    public static ApiResult<T> Succeeded(T? data, int status = StatusCodes.Status200OK)
        => new(data, true, status);

    public static ApiResult<T> Failed(object? error, int status)
        => new(default, false, status, error);

    public static ApiResult<T> Created(T? data, string? location)
        => new(data, true, StatusCodes.Status201Created, location: location);

    public static ApiResult<T> NoContent()
        => new(default, true, StatusCodes.Status204NoContent);

    public static implicit operator ApiResult<T>(T data) => Succeeded(data);

    public Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Items[typeof(ApiResult<>)] = true;

        if (location is not null)
            context.HttpContext.Response.Headers.Location = location;

        return new ObjectResult(this)
        {
            StatusCode = Status
        }.ExecuteResultAsync(context);
    }
}
