# Serilog logging

The API uses [Serilog](https://serilog.net) for structured logging. Logs go to the console and to daily rolling files.

## How it works

| Piece | Where |
|---|---|
| Package `Serilog.AspNetCore` (includes console and file sinks and the config reader) | `Shop.Api/Shop.Api.csproj` |
| `builder.Host.UseSerilog(...)` reads settings from config | `Shop.Api/Program.cs` |
| `app.UseSerilogRequestLogging()` writes one summary line per request | `Shop.Api/Program.cs` |
| Levels and sinks | `Serilog` section of `Shop.Api/appsettings.json` |
| Log files (git-ignored) | `Shop.Api/logs/shop-YYYYMMDD.log` |

`UseSerilog` replaces the default .NET logging, so everything that uses `ILogger<T>` (including framework logs) flows through Serilog. No changes are needed in existing code.

## Configuration

```json
"Serilog": {
  "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File" ],
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" },
    { "Name": "File", "Args": { "path": "logs/shop-.log", "rollingInterval": "Day", "retainedFileCountLimit": 14 } }
  ],
  "Enrich": [ "FromLogContext" ]
}
```

- **`MinimumLevel.Default`**: lowest level logged (`Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`).
- **`Override`**: per-namespace levels. Framework namespaces are set to `Warning` to cut noise. Add `"Microsoft.EntityFrameworkCore.Database.Command": "Information"` to see SQL.
- **File sink**: a new file each day (`shop-20261013.log`); files older than 14 days are deleted. The path is relative to the working directory, so `dotnet run --project Shop.Api` writes to `logs/` where the app was started.

To change levels per environment, override in `appsettings.Development.json`, e.g. `"Serilog": { "MinimumLevel": { "Default": "Debug" } }`.

## Writing logs

Inject `ILogger<T>` as usual and use message templates, not string interpolation:

```csharp
logger.LogInformation("Order {OrderId} placed by user {UserId}", order.Id, userId);
logger.LogError(ex, "Failed to persist exception log for {ExceptionType}", ex.GetType().Name);
```

The `{OrderId}` and `{UserId}` values are stored as properties, so they can be searched later instead of being flattened into text.

## Request logging

`UseSerilogRequestLogging()` replaces the several per-request framework lines with one line, for example:

```
HTTP GET /api/v1/products responded 200 in 14.2 ms
```

It sits before `ExceptionHandlingMiddleware`, so requests that end in a handled error are still logged, with the resulting status code.

## Relationship with Sentry and Mongo

- **Sentry** is not connected to Serilog. Errors reach Sentry only through the explicit `SentrySdk.CaptureException` call in `ExceptionHandlingMiddleware` (500s only). Because Serilog replaces the default logging, Sentry's automatic `LogError` capture no longer applies. To send error logs to Sentry as well, add the `Sentry.Serilog` package and a `Sentry` entry in `WriteTo`.
- **MongoDB** exception logs are unchanged. They are a separate, queryable store for handled exceptions, while Serilog holds the general application log.

## Common tasks

- **See more detail while debugging**: set `Serilog:MinimumLevel:Default` to `Debug`.
- **Turn off file logging**: remove the `File` entry from `WriteTo`.
- **Add another sink** (Seq, Elasticsearch, …): add the sink's NuGet package, then add its name to `Using` and an entry to `WriteTo`.
- **Logs folder is missing**: it is created on the first write. Check the app's working directory.
