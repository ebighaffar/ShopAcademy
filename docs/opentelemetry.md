# OpenTelemetry

The API exports logs, traces and metrics over OTLP, the OpenTelemetry protocol, to an [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/). The Collector forwards them to Grafana, and sends a copy of the traces to Seq.

```
API ── OTLP ──► Collector ─┬─ logs, traces, metrics ──► otel-lgtm (Grafana)
                           └─ traces ─────────────────► Seq
API ── Serilog Seq sink ── logs ──────────────────────► Seq
```

The API only knows the Collector's address. Backends can change in the Collector's config without touching application code.

## How it works

| Piece | Where |
|---|---|
| Traces and metrics registration (`AddOpenTelemetryExport`) | `Shop.Api/Telemetry/OpenTelemetryExtensions.cs` |
| Log export through the Serilog OTLP sink (`WriteToOpenTelemetry`) | `Shop.Api/Telemetry/OpenTelemetryExtensions.cs` |
| Settings | `OpenTelemetry` section of `Shop.Api/appsettings.json` |
| Custom spans and metrics | `Shop.Application/Telemetry` |
| 500 errors recorded on the active span | `Shop.Api/Middleware/ExceptionHandlingMiddleware.cs` |
| Collector config | `docs/observability/otel-collector.yaml` |
| Grafana board | `docs/observability/shop-api-dashboard.json` |
| Grafana stack and board JSON explained | [grafana.md](grafana.md) |
| Collector OTLP endpoints (4317, 4318) | [otlp-collector-api.md](otlp-collector-api.md) |

`UseSerilog` replaces the .NET logging providers, so OpenTelemetry's own log provider would receive nothing. That is why logs use a Serilog sink instead. `writeToProviders: true` is the other option, but it forwards logs to every registered provider, including Sentry's. The sink shares its endpoint and service identity with traces and metrics, so all three signals match.

## Configuration

```json
"OpenTelemetry": {
  "ServiceName": "ShopAPI",
  "ServiceVersion": "1.0.0",
  "Otlp": {
    "Endpoint": "http://localhost:4317",
    "Protocol": "grpc"
  },
  "Tracing": {
    "SamplingRatio": 1.0
  }
}
```

- **`Otlp:Endpoint`**: where telemetry is sent. Leave it blank to turn all export off.
- **`Otlp:Protocol`**: `grpc` (port 4317) or `http` (port 4318).
- **`Tracing:SamplingRatio`**: share of new traces to keep, from `0.0` to `1.0`. `1.0` keeps everything, which suits local work. It is a head-based, parent-based sampler, so a kept trace stays complete, but a low ratio can discard errors and slow requests.
- **Not traced**: `/swagger`, `/scalar` and `/openapi` requests.

## What is collected

### Automatic

| Instrumentation | Gives you |
|---|---|
| ASP.NET Core | A span per request, plus `http.server.request.duration` |
| Npgsql | A span per SQL command, with the SQL text but no parameter values |
| StackExchange.Redis (prerelease package) | A span per Redis command |
| HttpClient | Spans and metrics for outbound calls. The app makes none of its own yet |
| .NET runtime | Memory, GC and thread pool metrics |

MongoDB is not instrumented because it only stores exception logs. EF Core is not instrumented because Npgsql already covers the SQL.

### Custom metrics

The name in code is on the left. Prometheus changes dots to underscores and adds a suffix, so that is the name you query.

| Metric | Type | Tags | In Prometheus |
|---|---|---|---|
| `shop.auth.logins` | Counter | `outcome` | `shop_auth_logins_total` |
| `shop.auth.login.duration` | Histogram (ms) | `outcome` | `shop_auth_login_duration_milliseconds_bucket` |
| `shop.products.changes` | Counter | `operation` | `shop_products_changes_total` |
| `shop.cache.requests` | Counter | `result` | `shop_cache_requests_total` |

`outcome` is `success`, `failed` (rejected credentials) or `error` (something broke). `operation` is `created`, `updated` or `deleted`. `result` is `hit` or `miss`.

### Custom spans

| Span | Source | Tags |
|---|---|---|
| `AuthService.Login` | `ShopAPI.Auth` | `outcome`, `user.id` |
| `ProductService.Create`, `.Update`, `.Delete` | `ShopAPI.Catalog` | `product.id` |

A failed login is an expected rejection, so its span is not marked as an error. Only an unexpected exception is.

## Run the dashboards

These commands run in WSL. Seq must already be running (see [serilog.md](serilog.md#seq-dashboard)), and a WSL terminal has to stay open, because Docker can stop when WSL goes idle.

Create a network so the containers can reach each other by name, and add Seq to it:

```bash
docker network create shop-telemetry
docker network connect shop-telemetry seq
```

Start Grafana. `otel-lgtm` is one image with Grafana, Prometheus, Tempo and Loki inside. It is built for development and demos, not production. Its OTLP ports are not published, because the Collector uses them on the host:

```bash
docker run -d --name otel-lgtm --restart unless-stopped \
  --network shop-telemetry \
  -p 127.0.0.1:3000:3000 \
  -v otel-lgtm-data:/data \
  grafana/otel-lgtm:0.39.0
```

Start the Collector, using the path to this repository in WSL:

```bash
docker run -d --name otel-collector --restart unless-stopped \
  --network shop-telemetry \
  -p 127.0.0.1:4317:4317 -p 127.0.0.1:4318:4318 \
  -v /mnt/d/Academy/NetZtH/Sessions/Project/Shop/docs/observability/otel-collector.yaml:/etc/otelcol-contrib/config.yaml:ro \
  otel/opentelemetry-collector-contrib:0.164.0
```

Then run the API. Inside the Docker network, containers use names like `otel-lgtm`. The API runs on the host, so it uses `localhost`.

### Import the board

1. Open <http://localhost:3000> and sign in with `admin` / `admin`.
2. Go to **Dashboards → New → Import**.
3. Upload `docs/observability/shop-api-dashboard.json`.

The `-v otel-lgtm-data:/data` option keeps the board and data across restarts. Without it, you need to import the board again whenever the container is recreated.

## Using it

Metrics tell you something is wrong, traces show where, and logs explain why.

1. **Metrics**: on the **Shop API** board, find a spike in 5xx errors or latency. Metrics are exported every 60 seconds, and rate panels need two samples, so allow a couple of minutes after starting.
2. **Traces**: in **Recent traces**, click a Trace ID to see every span with its timing. You can also search in **Explore → Tempo**, for example `{status=error}` or `{name="AuthService.Login" && span.outcome="failed"}`.
3. **Logs**: in **Explore → Loki**, find the logs for a trace with `{service_name="ShopAPI"} | trace_id="<id>"`.

Traces can take around 30 seconds to become searchable in Tempo.

Useful Prometheus queries:

```promql
# Failed logins per minute
sum(rate(shop_auth_logins_total{outcome="failed"}[5m])) * 60

# p95 latency by route
histogram_quantile(0.95, sum by (le, http_route) (rate(http_server_request_duration_seconds_bucket[5m])))

# Cache hit ratio
sum(rate(shop_cache_requests_total{result="hit"}[5m])) / sum(rate(shop_cache_requests_total[5m]))
```

Seq shows the same traces next to its logs at <http://localhost:5341>. Every log event from the API includes its `TraceId`, so `@TraceId = '<id>'` finds the logs and spans for one request. The console and file logs print it too, and error responses return it as `traceId`. See [Finding logs by trace ID](serilog.md#finding-logs-by-trace-id).

## Adding a metric or span

Add it to a class in `Shop.Application/Telemetry` (or create a new one), then record it inside the operation:

```csharp
private static readonly Counter<long> OrdersPlaced = Meter.CreateCounter<long>("shop.orders.placed");

public static void RecordOrderPlaced(string paymentMethod)
    => OrdersPlaced.Add(1, new KeyValuePair<string, object?>("payment_method", paymentMethod));
```

```csharp
using var activity = OrderTelemetry.Source.StartActivity("OrderService.Place");
activity?.SetTag("order.id", order.Id);
```

- A new source or meter name must be added to `AddSource` and `AddMeter` in `OpenTelemetryExtensions.cs`. Otherwise its data is dropped with no error.
- `StartActivity` returns `null` when nothing is listening, so use `?.`.
- Keep metric tags to a small, fixed set of values. Put ids such as `order.id` on spans, because each new tag value creates a new time series.
- Don't add a span to every method. Add them around meaningful operations.

## When telemetry is down

Telemetry should never break a request. Exporting happens in the background, in batches.

- **Collector or Grafana is down**: requests carry on as normal. The exporters retry in the background and drop what doesn't fit in their queues. Delivery resumes on its own when the service is back.
- **Redis is down**: the API still starts, because the connection keeps retrying in the background. Calls that use the cache fail after the Redis timeout of about 5 seconds.

To see this yourself, run `docker stop otel-collector`, send some requests, compare response times, then run `docker start otel-collector` and watch new traces arrive.

Delivery is not guaranteed: anything still queued is lost if a process crashes.

## Relationship with Serilog and Sentry

- **Serilog** stays the logging API. Its console, file and Seq sinks are unchanged, and the OTLP sink adds a copy for Grafana. It uses the same `MinimumLevel`, so `Debug` logs also go to Grafana. Seq doesn't get logs from the Collector, because it would then store them twice.
- **Sentry** is unchanged and still reports 500s only. The same 500 now also shows on its trace, with the exception attached.
- **MongoDB** exception logs are unchanged.

## Common tasks

- **Turn telemetry off**: set `OpenTelemetry:Otlp:Endpoint` to blank.
- **Send straight to Grafana, without the Collector**: stop `otel-collector` and publish `4317` and `4318` on `otel-lgtm`. The API config doesn't change.
- **Try sampling**: set `Tracing:SamplingRatio` to `0.1` and send 100 or more requests. Roughly one in ten appears in Tempo, while metrics and logs stay complete.
- **Add a backend**: add an exporter in `otel-collector.yaml`, add it to a pipeline, then run `docker restart otel-collector`.
- **Go further with the Collector**: the `contrib` image also has `filter` and `redaction` processors, and `tail_sampling`, which keeps errors and slow traces. Tail sampling only sees what head sampling kept, so plan the two together.

## Troubleshooting

- **Nothing in Grafana**: check `docker ps` shows `otel-collector` and `otel-lgtm`, then read `docker logs otel-collector`.
- **`HTTP_PROXY` is set**: .NET sends the exporter's traffic to the proxy, so telemetry silently goes nowhere. This happens with a VPN or proxy tool that sets the variable. Set `NO_PROXY=localhost,127.0.0.1` for the API, or unset the variable.
- **A custom metric or span is missing**: check its name is in `AddMeter` or `AddSource`, and that the code path actually ran.
- **Rate panels are empty but counters have values**: wait for a second export, around 2 minutes.
- **Port already allocated on 4317**: the Aspire dashboard from the lab uses the same host port. Run one at a time.
- **`column ... does not exist` errors in traces**: the database is behind the code. Run `dotnet ef database update`.
