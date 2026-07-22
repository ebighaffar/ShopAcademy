namespace Shop.Api.Contracts.ExceptionLogs;

public record ExceptionLogResponse(
    string Id,
    string ExceptionType,
    string Message,
    string? StackTrace,
    string? Source,
    string? RequestPath,
    string? RequestMethod,
    int StatusCode,
    DateTimeOffset CreatedAt);
