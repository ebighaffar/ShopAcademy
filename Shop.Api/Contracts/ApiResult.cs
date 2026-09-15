using Microsoft.AspNetCore.Mvc;

namespace Shop.Api.Contracts;

public sealed class ApiResult(
    bool success,
    int status,
    object? error = null,
    string? location = null
) : IActionResult
{
    public bool Success { get; } = success;
    public int Status { get; } = status;
    public object? Error { get; } = error;

    public static ApiResult Succeeded(int status = StatusCodes.Status200OK)
        => new(true, status);

    public static ApiResult Failed(object? error, int status)
        => new(false, status, error);

    public static ApiResult NoContent()
        => new(true, StatusCodes.Status204NoContent);

    public Task ExecuteResultAsync(ActionContext context)
    {
        context.HttpContext.Items[typeof(ApiResult)] = true;

        if (location is not null)
            context.HttpContext.Response.Headers.Location = location;

        return new ObjectResult(this)
        {
            StatusCode = Status
        }.ExecuteResultAsync(context);
    }
}
