# Sentry error monitoring

Unexpected server errors (HTTP 500) are reported to [Sentry](https://sentry.io) and shown in its web dashboard. Expected domain errors (404, 409, 400, 401…) are **not** reported.

## How it works

| Piece | Where |
|---|---|
| Package `Sentry.AspNetCore` | `Shop.Api/Shop.Api.csproj` |
| SDK startup (`builder.WebHost.UseSentry()`) | `Shop.Api/Program.cs` |
| Config section `Sentry` (only `Dsn`) | `Shop.Api/appsettings.json` |
| Report 500s with `SentrySdk.CaptureException` | `Shop.Api/Middleware/ExceptionHandlingMiddleware.cs` |
| Dev-only test endpoint | `MapSentryTestEndpoint` in `Shop.Api/Extensions/WebApplicationExtensions.cs` |

`ExceptionHandlingMiddleware` catches every exception and turns it into a response, so Sentry's built-in capture never sees them. That is why the middleware calls `SentrySdk.CaptureException` itself, and only when the status resolves to 500.

The existing MongoDB exception log is unchanged. Sentry runs alongside it.

Performance tracing is off (no `TracesSampleRate`); only errors are sent. Sentry attaches request details (URL, method, headers) automatically. Personal data (user IP, cookies) is not sent, because `SendDefaultPii` is off by default.

**If the DSN is empty, Sentry is disabled and the app runs normally.**

## Setup

1. Sign up at <https://sentry.io> (the free tier is enough).
2. Create a project: platform **ASP.NET Core**, name it e.g. `shop-api`.
3. Copy the **DSN** (Settings → Projects → shop-api → Client Keys (DSN)).
4. Put it in `Shop.Api/appsettings.json`:

   ```json
   "Sentry": { "Dsn": "https://<key>@o<org>.ingest.sentry.io/<project>" }
   ```

   This project is not deployed to production, so the DSN is committed in config. If that changes, move it to user-secrets (`dotnet user-secrets set "Sentry:Dsn" "<dsn>" --project Shop.Api`) or the `Sentry__Dsn` environment variable.

5. Optional: `Sentry:Environment` defaults to the ASP.NET environment (`Development`, `Production`), which is what the dashboard's environment filter uses.

## Verify it works

1. Run the API: `dotnet run --project Shop.Api --launch-profile https`
2. Open <https://localhost:7024/debug/sentry-test> (exists only in Development; it throws on purpose).
3. You get a 500 response with "An unexpected error occurred."
4. Within a few seconds a new issue **InvalidOperationException: Sentry test exception** appears in the dashboard under **Issues**.

## Using the dashboard

- **Issues**: errors grouped by cause. Open one to see the stack trace, request URL/method, environment, and how often and when it happened.
- **Filters**: use the environment dropdown (`Development` / `Production`) to separate local noise from real errors.
- **Resolve / Ignore**: mark an issue resolved once fixed; Sentry re-opens it if it comes back.
- **Alerts** (Alerts → Create Alert): by default Sentry emails you about new issues. Add rules to notify Slack or others.
- **Releases** are not configured. To enable them later, set `Sentry:Release`.

## Changing what is reported

- Report 4xx domain errors too: remove the `if (statusCode == HttpStatusCode.InternalServerError)` condition in `ExceptionHandlingMiddleware`.
- Turn on performance tracing: add `"TracesSampleRate": 0.1` under `Sentry` in config.
- Turn Sentry off: leave `Sentry:Dsn` empty.
