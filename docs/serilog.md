# Serilog logging

The API uses [Serilog](https://serilog.net) for structured logging. Logs go to the console, to daily rolling files, and to [Seq](https://datalust.co/seq), a local web dashboard for searching them (see [Seq dashboard](#seq-dashboard)).

## How it works

| Piece | Where |
|---|---|
| Packages `Serilog.AspNetCore` (console and file sinks, config reader) and `Serilog.Sinks.Seq` | `Shop.Api/Shop.Api.csproj` |
| `builder.Host.UseSerilog(...)` reads settings from config | `Shop.Api/Program.cs` |
| `app.UseSerilogRequestLogging()` writes one summary line per request | `Shop.Api/Program.cs` |
| Levels and sinks | `Serilog` section of `Shop.Api/appsettings.json` |
| Log files (git-ignored) | `Shop.Api/logs/shop-YYYYMMDD.log` |

`UseSerilog` replaces the default .NET logging, so everything that uses `ILogger<T>` (including framework logs) flows through Serilog. No changes are needed in existing code.

## Configuration

```json
"Serilog": {
  "Using": [ "Serilog.Sinks.Console", "Serilog.Sinks.File", "Serilog.Sinks.Seq" ],
  "MinimumLevel": {
    "Default": "Information",
    "Override": {
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "WriteTo": [
    { "Name": "Console" },
    { "Name": "File", "Args": { "path": "logs/shop-.log", "rollingInterval": "Day", "retainedFileCountLimit": 14 } },
    { "Name": "Seq", "Args": { "serverUrl": "http://localhost:5341" } }
  ],
  "Enrich": [ "FromLogContext" ]
}
```

- **`MinimumLevel.Default`**: lowest level logged (`Verbose`, `Debug`, `Information`, `Warning`, `Error`, `Fatal`).
- **`Override`**: per-namespace levels. Framework namespaces are set to `Warning` to cut noise. Add `"Microsoft.EntityFrameworkCore.Database.Command": "Information"` to see SQL.
- **File sink**: a new file each day (`shop-20261013.log`); files older than 14 days are deleted. The path is relative to the working directory, so `dotnet run --project Shop.Api` writes to `logs/` where the app was started.

To change levels per environment, override in `appsettings.Development.json`, e.g. `"Serilog": { "MinimumLevel": { "Default": "Debug" } }`.

## Seq dashboard

Serilog has no dashboard of its own. Seq is a log server with a web UI that is built for Serilog. It runs locally in Docker (free for a single user), so nothing leaves your machine.

### Run Seq (WSL / Docker)

```bash
docker run -d --name seq --restart unless-stopped \
  -e ACCEPT_EULA=Y \
  -p 5341:80 \
  -v seq-data:/data \
  datalust/seq
```

- UI and ingestion share one port: open <http://localhost:5341>.
- `-v seq-data:/data` keeps logs across container restarts; leave it out for throwaway logs.
- Later: `docker stop seq` / `docker start seq`. To remove it: `docker rm -f seq`.
- With a first-time setup Seq runs without a login. For anything beyond local use, set an admin password (see Seq's docs).

### Using it

1. Start Seq, then start the API. The `Seq` sink sends events in the background.
2. In the UI, the **Events** page shows the live stream. Click an event to see its properties.
3. Search with the filter box, for example:
   - `@Level = 'Error'`: only errors
   - `RequestPath like '/api/v1/products%'`: logs for one route
   - `StatusCode >= 500`: failed requests (from the request log line)
   - `OrderId = 42`: any event that logged that property
4. Use **Dashboards** and **Alerts** (Settings) to chart counts and get notified.

If Seq is not running, the app is unaffected: the sink retries in the background and the console and file sinks still work. It only buffers briefly, so events logged while Seq is down may be lost.

To point at another Seq server, change `serverUrl` (add `"apiKey": "..."` if the server requires one).

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
- **Turn off file or Seq logging**: remove its entry from `WriteTo`.
- **Add another sink** (Elasticsearch, …): add the sink's NuGet package, then add its name to `Using` and an entry to `WriteTo`.
- **Nothing appears in Seq**: check `docker ps` shows `seq`, <http://localhost:5341> opens, and `serverUrl` matches the port you published.
- **Logs folder is missing**: it is created on the first write. Check the app's working directory.
