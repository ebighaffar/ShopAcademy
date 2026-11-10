# Grafana stack

Grafana is where the API's telemetry is read. It does not store anything itself. It queries three storage backends, one per signal, and joins them by trace ID. In this project all of them run inside one container, `otel-lgtm`. For how data gets there, see [opentelemetry.md](opentelemetry.md). For the Collector's own endpoints, see [otlp-collector-api.md](otlp-collector-api.md).

```
                       ┌──────────────────── otel-lgtm container ───────────────────┐
API ─► Collector ─OTLP─►  OTel Collector ─┬─ metrics ─► Prometheus ─┐                │
                       │  (inside image)  ├─ traces  ─► Tempo ──────┼─► Grafana :3000 ◄── you
                       │                  └─ logs    ─► Loki ───────┘                │
                       └─────────────────────────────────────────────────────────────┘
```

## How it fits together

### Who does what

| Tool | Job | Question it answers |
|---|---|---|
| **Prometheus** | Stores **numbers over time** | "Is something wrong?" |
| **Tempo** | Stores the **journey of one request** | "Where is it slow or broken?" |
| **Loki** | Stores **text log lines** | "What did the app say about it?" |
| **Grafana** | **Shows** all three on one screen | The window you look through |

Grafana stores nothing. It only asks the other three for data and draws it.

A restaurant makes a good comparison:

- **Prometheus** is the manager's clipboard: "120 orders per hour, average wait 8 minutes, 3 complaints." It gives totals and trends, but not who or why.
- **Tempo** is a stopwatch on one order: "Order #57: 2 min to order, 12 min in the kitchen, 1 min to deliver." It shows where the time went.
- **Loki** is the diary: "12:03, the chef said the grill is broken." It gives the reason in plain words.
- **Grafana** is the control-room screen that shows all three together.

Each kind of data has a different shape, so each has its own store. Numbers are tiny and need math such as rates and percentiles. Traces are trees of timed steps. Logs are large piles of text, so Loki indexes only a few labels like `service_name` to stay cheap. One database doing all three would be slow or expensive at each.

### A worked example

Customers say "login is slow".

1. **Metrics.** On the Shop API board, the *Request latency* p95 jumped from 100 ms to 3 s at 14:05. Something is wrong, but not where.
2. **Trace.** Open a slow trace from 14:05 in **Recent traces**. It shows 2.9 s of the 3 s went to one Npgsql query. Now you know where.
3. **Logs.** Open the logs for that trace ID. You see `Npgsql: timeout waiting for connection from pool`. Now you know why.

The order is always metrics (that something is wrong), then trace (where), then logs (why).

The three are linked by the **Trace ID**. Each log line from the API carries the `trace_id` of the request that produced it, which is what lets Grafana jump from a trace to its logs. All three signals also carry the same `service.name` (`ShopAPI`), so they line up.

### What OpenTelemetry does, and what these tools do

OpenTelemetry only **collects and delivers**. It keeps nothing. If the Collector has nothing behind it, the data is thrown away.

| Step | Who | Job |
|---|---|---|
| 1. Produce | **OTel SDK inside the API** | Creates the numbers, spans and log lines |
| 2. Deliver | **OTel Collector** | Receives them, batches them and forwards them |
| 3. Store | **Prometheus, Tempo, Loki** | Keep the data on disk, organized so it can be searched |
| 4. Query | **Prometheus, Tempo, Loki** | Answer questions such as "p95 latency for the last hour" |
| 5. Display | **Grafana** | Sends those questions and draws the answers |

Steps 3 and 4 are why the backends exist. Without them, last Tuesday's latency is gone, because OTel sent it and nobody kept it. They also do the calculating: Prometheus computes `histogram_quantile(0.95, …)` over millions of samples, and Tempo finds the one trace with `status = error` among thousands. Grafana only asks.

Put another way: OTel is the delivery truck, the three backends are the warehouses, and Grafana is the shop window. Seq in this project is both a warehouse and its own window, because it stores logs and traces and has a built-in UI at <http://localhost:5341>.

### Do they have their own dashboards?

| Tool | Own UI? |
|---|---|
| **Prometheus** | Yes, a basic one on port `9090`: a query box, a simple graph, and pages for targets and settings. It suits quick checks, not building boards |
| **Tempo** | No. API only |
| **Loki** | No. API only |

Without Grafana you would query Tempo and Loki with `curl` and read raw JSON, which is why Grafana is the common screen for all three.

Here Prometheus's `9090` is not published to the host, so you reach it through **Explore → Prometheus**. To open its own UI, add `-p 127.0.0.1:9090:9090` to the `otel-lgtm` command.

### Push, not pull

**OTel is push-based.** The API's exporter batches data and posts it to the Collector on its own (metrics every 60 seconds, traces and logs in small batches). Nothing comes to fetch it.

Prometheus by itself is **pull-based**: it visits an app's `/metrics` page every few seconds and copies what it finds. This project doesn't do that. Metrics arrive by push, through the Collector, and Prometheus only stores them. Both styles work. Push is simpler here because a single pipe carries all three signals.

### How each signal finds its backend

Nothing guesses from the content. **The signal type is stated explicitly at every step.**

1. **The API labels each signal when it sends it**, using a different endpoint or gRPC service per type:

    | Signal | HTTP path | gRPC service |
    |---|---|---|
    | Traces | `/v1/traces` | `TraceService` |
    | Metrics | `/v1/metrics` | `MetricsService` |
    | Logs | `/v1/logs` | `LogsService` |

2. **The Collector routes by type with fixed wiring**, in the `pipelines` block of `otel-collector.yaml`:

    ```yaml
    traces:  receivers: [otlp] → exporters: [grafana, seq]
    metrics: receivers: [otlp] → exporters: [grafana]
    logs:    receivers: [otlp] → exporters: [grafana]
    ```

    A trace that arrives on `/v1/traces` can only enter the `traces` pipeline. It never reaches Prometheus.

3. **Inside `otel-lgtm` the same split happens again.** The Collector sends all three signals to one address, `otel-lgtm:4317`, and the image's built-in collector separates them by type into Prometheus, Tempo and Loki. This is how the image is documented to work. It is not verified here by looking inside the container.

```
API (SDK creates metrics, spans, logs)
   │ push, each tagged with its type and service.name
   ▼
Collector ── by type ──► metrics pipeline ─┐
                    ├──► traces pipeline  ─┼──► otel-lgtm ── by type ──► Prometheus / Tempo / Loki
                    └──► logs pipeline    ─┘
```

### How a metric exists in the first place

The API creates metrics on purpose, and nothing is discovered.

- **Automatic**: the ASP.NET Core, HttpClient and Runtime instrumentation in `OpenTelemetryExtensions.cs` create standard metrics such as `http.server.request.duration`.
- **Custom**: the code creates them, for example `Meter.CreateCounter<long>("shop.cache.requests")`.
- **Registered**: `AddMeter(AuthTelemetry.Name, CatalogTelemetry.Name)` tells the SDK which custom meters to export. A meter that isn't listed is silently dropped.

Each metric carries its own name, type (counter or histogram), unit and labels such as `outcome="failed"`. Prometheus stores it as the name plus its labels. The `service.name` the API attaches becomes the `service_name` label, so a query can pick out one service.

## The tools

| Tool | Signal | Query language | What it answers |
|---|---|---|---|
| **Grafana** | all | n/a | The UI: boards, Explore, links between signals |
| **Prometheus** | Metrics | PromQL | How many, how fast, how often. Numbers over time |
| **Tempo** | Traces | TraceQL | Where did this one request spend its time |
| **Loki** | Logs | LogQL | What did the app say while it happened |
| **OpenTelemetry Collector** | transport | n/a | Receives OTLP and routes it to the three backends |

The `grafana/otel-lgtm` image name spells it out: **L**oki, **G**rafana, **T**empo, **M**imir/Prometheus. It is built for development and demos. It is not a production setup: one container, one volume, no replication and no auth hardening.

### Prometheus

Stores metrics as time series: a metric name plus a set of labels, with a value sampled over time.

- It receives metrics pushed over OTLP. It does not scrape the API.
- Dots in OTel names become underscores, and a unit or type suffix is added: `shop.cache.requests` becomes `shop_cache_requests_total`. The full list is in [opentelemetry.md](opentelemetry.md#custom-metrics).
- The OTel resource attribute `service.name` becomes the label `service_name`. Every panel in the board filters on it.
- The API exports every 60 seconds, so a rate needs two samples before it shows anything.
- Each distinct label value is a new series. That is why metric tags stay low-cardinality and ids go on spans.

### Tempo

Stores traces. A trace is the whole journey of one request, made of spans: the HTTP request, then the Npgsql query, then the Redis call, and so on.

- A trace is looked up by its Trace ID, or searched with TraceQL.
- Resource attributes are addressed as `resource.service.name`. Span attributes as `span.outcome`.
- Spans can take about 30 seconds to become searchable.

### Loki

Stores logs. It indexes only a few labels (here `service_name`) and keeps the text as-is, so queries narrow by label first and then filter the text.

- Logs reach it from the Serilog OTLP sink, with the active `trace_id` attached. That is what lets Grafana jump from a log line to its trace.
- The Collector sends logs to Grafana only. Seq gets its own copy from the Serilog Seq sink.

### Grafana

Holds no telemetry. It holds **data sources** (connections to the three above), **dashboards** (JSON, see below) and **Explore** (ad-hoc queries).

In `otel-lgtm` the data sources are already provisioned, with fixed UIDs that the board depends on:

| Data source | UID | Type |
|---|---|---|
| Prometheus | `prometheus` | `prometheus` |
| Tempo | `tempo` | `tempo` |
| Loki | `loki` | `loki` |

If you point the board at a Grafana you set up yourself, create data sources with these same UIDs, or the panels show "datasource not found".

## Ports

| Port | Service | Published to the host here? |
|---|---|---|
| `3000` | Grafana UI | Yes, on `127.0.0.1` only |
| `4317`, `4318` | The image's own OTLP receiver | No. The standalone Collector owns these on the host |
| Others (Prometheus, Loki, Tempo HTTP APIs) | Backends | No. Reach them through Grafana |

Sign in at <http://localhost:3000> with `admin` / `admin`. This is the image default, and it is acceptable only because the port is bound to `127.0.0.1`.

Startup commands are in [opentelemetry.md](opentelemetry.md#run-the-dashboards).

## Explore: the routine queries

Open **Explore**, pick the data source in the top-left, and paste.

### PromQL (Prometheus)

```promql
# Request rate by route
sum by (http_route) (rate(http_server_request_duration_seconds_count{service_name="ShopAPI"}[5m]))

# 5xx share of all requests
sum(rate(http_server_request_duration_seconds_count{service_name="ShopAPI",http_response_status_code=~"5.."}[5m]))
  / sum(rate(http_server_request_duration_seconds_count{service_name="ShopAPI"}[5m]))

# p95 latency by route
histogram_quantile(0.95, sum by (le, http_route) (rate(http_server_request_duration_seconds_bucket{service_name="ShopAPI"}[5m])))

# Failed logins per minute
sum(rate(shop_auth_logins_total{outcome="failed"}[5m])) * 60

# Cache hit ratio
sum(rate(shop_cache_requests_total{result="hit"}[5m])) / sum(rate(shop_cache_requests_total[5m]))
```

To find which metrics exist, type `shop_` in the metric picker, or run `{__name__=~"shop_.*"}`.

### TraceQL (Tempo)

```traceql
# Every trace that contains an error
{ status = error }

# Slow requests
{ resource.service.name = "ShopAPI" && duration > 500ms }

# Rejected logins
{ name = "AuthService.Login" && span.outcome = "failed" }

# Everything that touched one product
{ span.product.id = 42 }
```

### LogQL (Loki)

```logql
# All API logs
{service_name="ShopAPI"}

# Only logs for one trace
{service_name="ShopAPI"} | trace_id="<trace id>"

# Text filter
{service_name="ShopAPI"} |= "Exception"

# Error volume per minute
sum(count_over_time({service_name="ShopAPI"} |= "Error" [1m]))
```

## The dashboard JSON

`docs/observability/shop-api-dashboard.json` is the whole board as one JSON document. Grafana reads and writes this format, so the file can be imported, edited by hand and reviewed in git.

### Top level

| Field | Value | Meaning |
|---|---|---|
| `uid` | `shop-api` | Stable ID. The URL is `/d/shop-api`. Re-importing the same `uid` replaces the board instead of duplicating it |
| `title` | `Shop API` | Name shown in the UI |
| `tags` | `shop`, `opentelemetry` | For search and filtering |
| `schemaVersion` | `41` | Grafana's internal format version. Grafana migrates older ones forward. Don't edit by hand |
| `version` | `1` | Revision counter. Grafana bumps it on each save |
| `editable` | `true` | Allows changing panels in the UI |
| `graphTooltip` | `1` | Shared crosshair: hovering one chart marks the same time on all |
| `refresh` | `30s` | Auto-refresh interval |
| `time` | `now-30m` to `now` | Default time range |
| `timezone` | `browser` | Use the viewer's timezone |
| `templating` | one variable | Dropdowns at the top. See below |
| `panels` | 16 entries | Rows and charts. See below |

### Variable: `service`

```json
{
  "name": "service",
  "type": "query",
  "datasource": { "type": "prometheus", "uid": "prometheus" },
  "query": { "query": "label_values(shop_auth_logins_total, service_name)", "refId": "v" },
  "refresh": 2,
  "includeAll": false,
  "multi": false
}
```

It fills a dropdown with every `service_name` seen on `shop_auth_logins_total`, and panels use it as `$service`. `refresh: 2` reloads the list whenever the time range changes. If a panel ever shows "No data" while the API runs, check that this dropdown actually has a value. It is empty until the first login has been recorded.

### Layout

Grafana's grid is **24 columns** wide. Each panel has a `gridPos`:

```json
"gridPos": { "h": 8, "w": 10, "x": 0, "y": 1 }
```

`x` and `w` are columns, `y` and `h` are rows (a row is about 30 px). Panels on the same `y` sit side by side, and their widths add up to 24. The first row is 10 + 4 + 10.

### Panel anatomy

```json
{
  "id": 2,
  "type": "timeseries",
  "title": "Request rate by route",
  "description": "Shown as a tooltip next to the title.",
  "datasource": { "type": "prometheus", "uid": "prometheus" },
  "gridPos": { },
  "fieldConfig": { "defaults": { "unit": "reqps", "min": 0 }, "overrides": [] },
  "options": { "legend": { }, "tooltip": { } },
  "targets": [
    { "refId": "A", "expr": "…", "legendFormat": "{{http_route}}", "range": true }
  ]
}
```

| Part | What it controls |
|---|---|
| `id` | Unique within the board. Must not repeat |
| `type` | `timeseries`, `stat`, `table`, `logs` or `row` |
| `datasource` | Where `targets` run. Set on the panel and again on each target |
| `fieldConfig.defaults` | How values look: `unit`, `min`/`max`, `decimals`, `thresholds`, line style |
| `fieldConfig.overrides` | Per-series exceptions. Empty here |
| `options` | Type-specific settings: legend, tooltip, how a stat reduces its series |
| `targets` | The queries. `refId` names them (`A`, `B`), and each one is a line on the chart |
| `legendFormat` | Series name template. `{{label}}` inserts a label value |

### Query macros

| Macro | Expands to | Used for |
|---|---|---|
| `$service` | The dropdown choice | Filtering on `service_name` |
| `$__rate_interval` | A safe window for `rate()`, at least 4x the scrape interval | Time series panels. Avoids empty graphs on short windows |
| `$__range` | The whole selected time range | Stat panels, which show one number for the range |

`$__rate_interval` matters here because metrics arrive every 60 seconds. A fixed `[1m]` window holds one sample, which is too few for `rate()`.

### Panels

| id | Type | Title | Data source | What it shows |
|---|---|---|---|---|
| 1 | row | Traffic, errors and latency | n/a | Section header |
| 2 | timeseries | Request rate by route | Prometheus | Requests per second per route template |
| 3 | stat | 5xx error rate | Prometheus | Share of requests ending in 5xx. Green below 1%, orange 1%, red 5% |
| 4 | timeseries | Request latency | Prometheus | p95 and p99, two targets |
| 5 | row | Custom metrics | n/a | Section header |
| 6 | timeseries | Logins by outcome | Prometheus | `shop.auth.logins` per minute by `outcome` |
| 7 | timeseries | Login duration p95 | Prometheus | `shop.auth.login.duration` by `outcome` |
| 8 | timeseries | Product changes | Prometheus | `shop.products.changes` by `operation` |
| 9 | stat | Cache hit ratio | Prometheus | Hits over all lookups. Red below 50%, orange 50%, green 80% |
| 10 | row | Runtime saturation | n/a | Section header |
| 11 | timeseries | Memory | Prometheus | `dotnet_process_memory_working_set_bytes` |
| 12 | timeseries | Thread pool queue | Prometheus | `dotnet_thread_pool_queue_length_total` |
| 13 | timeseries | GC pause time | Prometheus | Share of each second paused for garbage collection |
| 14 | row | Drill down | n/a | Section header |
| 15 | table | Recent traces | Tempo | Latest 20 traces. Click a Trace ID to open it |
| 16 | logs | Logs | Loki | Latest API logs, with a link from a line to its trace |

Rows (`type: row`) are plain headers. They have `collapsed: false` and an empty `panels` list, so they don't hide anything.

### Details worth knowing

- **5xx error rate** is `(sum(rate(…5xx…)) or vector(0)) / sum(rate(…all…))`. The `or vector(0)` gives `0` instead of "No data" when there are no errors at all.
- **Thresholds** are `steps`. Each step's color applies from its `value` upward, and the first step has `value: null` meaning "from minus infinity".
- **Units** are Grafana unit IDs, not free text: `reqps`, `percentunit` (0 to 1 shown as a percentage), `s`, `ms`, `cpm` (count per minute) and `bytes`.
- **Login duration** uses `_milliseconds_bucket` because the histogram is recorded in ms, while HTTP latency uses `_seconds_bucket`.

### Import, edit, export

**Import**: **Dashboards → New → Import → Upload JSON file**. Grafana asks nothing else, because data sources are referenced by fixed UID.

**Edit in the UI**: make the change, then **Save**. The stored copy lives in the `otel-lgtm-data` volume, so it survives restarts but is **not** the file in git.

**Get your change back into git**: open the board, then **Export → Export as JSON**, leave "Export for sharing externally" **off** (it would replace the UIDs with `${DS_…}` placeholders), and copy the JSON over `shop-api-dashboard.json`. Grafana rewrites the whole file in its own order and bumps `version`, so expect a large diff on the first export.

**Edit by hand**: safe for queries, titles, thresholds and `gridPos`. Keep each `id` unique and each `refId` unique within its panel.

### Provision it instead of importing

To have the board load automatically on every container start, mount a provider file and the JSON into Grafana. The paths below are the standard Grafana provisioning locations. Check them against the image version before relying on this.

```yaml
# /etc/grafana/provisioning/dashboards/shop.yaml
apiVersion: 1
providers:
  - name: shop
    folder: Shop
    type: file
    disableDeletion: false
    allowUiUpdates: true
    options:
      path: /var/lib/grafana/dashboards
```

```bash
docker run -d --name otel-lgtm ... \
  -v /mnt/d/.../docs/observability/shop-api-dashboard.json:/var/lib/grafana/dashboards/shop-api-dashboard.json:ro \
  -v /mnt/d/.../docs/observability/shop-dashboards.yaml:/etc/grafana/provisioning/dashboards/shop.yaml:ro \
  grafana/otel-lgtm:0.39.0
```

A provisioned board is re-read from the file, so UI edits are overwritten unless saved back to the file.

## Managing the board in the UI

The JSON file is how Grafana saves what you build in the UI. You can build the whole board by clicking, or write the JSON by hand, or mix both. They are two views of the same thing.

### What you change where

| You want to change | In the Grafana UI | In the JSON |
|---|---|---|
| **A query** (what data a chart shows) | Edit panel → query box under the chart | `targets[].expr` |
| **Values**: unit, decimals, min/max, colors, thresholds | Edit panel → options on the right (Standard options, Thresholds) | `fieldConfig.defaults` |
| **Chart type** (line, stat, table, logs) | Edit panel → visualization picker, top right | `type` |
| **Layout**: position and size | Drag and resize panels on the board | `gridPos` |
| **Structure**: sections | Add → Row | panels with `type: "row"` |
| **Dropdowns** (like `Service`) | Dashboard settings → Variables | `templating.list` |
| **Time range and refresh** | Pickers at the top right | `time`, `refresh` |
| **Connections to Prometheus, Tempo and Loki** | Connections → Data sources | provisioning files, not the board JSON |

### Editing in the UI

1. Open the board and click **Edit** (top right).
2. Hover a panel and press `e`, or use its menu → **Edit**. Change the query, unit or thresholds and the chart updates as you type.
3. Click **Save dashboard**.

To add a panel, use **Add → Visualization**, choose the data source (Prometheus, Tempo or Loki) and write the query.

**Explore** is the easiest place to try a query first. When it works, use **Add to dashboard** to turn it into a panel.

### Seeing the JSON from the UI

You don't have to leave Grafana to read or edit the JSON:

- **Dashboard settings → JSON Model**: the whole board's JSON. You can edit it here and save.
- **Panel menu → Inspect → Panel JSON**: the JSON of one panel.
- **Export → Export as JSON**: a file to put in git. Leave "Export for sharing externally" off, as described in [Import, edit, export](#import-edit-export).

A useful routine: build or adjust a panel with the mouse, then copy its JSON into `shop-api-dashboard.json`.

### What Grafana can't manage

Grafana doesn't hold the telemetry, so it can't edit or delete it. It controls only how data is **queried and displayed**. Anything about the data itself is changed elsewhere:

| About the data | Where it is set |
|---|---|
| How long it is kept | Retention in Prometheus, Tempo and Loki |
| What is collected | The API code and `otel-collector.yaml` |
| Which metrics exist | The API creates them. Grafana can only show what Prometheus already has |

### UI or JSON?

- **UI** for creating and experimenting. You see the result immediately.
- **JSON in git** for the version you keep. It can be reviewed and survives a recreated container.

A board saved in the UI is stored in the `otel-lgtm-data` volume, **not** in the repo file. If you change panels and don't export, `docs/observability/shop-api-dashboard.json` is out of date. A sensible routine is to edit in the UI, then **Export as JSON** and overwrite the repo file when you're happy with it.

## Reading the board

Work left to right and top to bottom. Each row narrows the question.

1. **Traffic, errors and latency**: is something wrong? A 5xx rate above 1% or a p95 that has climbed is the signal.
2. **Custom metrics**: is it a business problem? Failed logins rising, product writes stopping or the cache hit ratio dropping.
3. **Runtime saturation**: is it the process? Memory climbing without falling back, a thread pool queue that keeps growing, or long GC pauses.
4. **Drill down**: open a slow or failed trace in **Recent traces**, read its spans, then expand a log line for the same `trace_id`.

## Troubleshooting

- **Every panel says "No data"**: check the `Service` dropdown has a value. Then check the API has run for at least two minutes and that `docker logs otel-collector` shows no export errors.
- **"Datasource … was not found"**: the data source UIDs differ from `prometheus`, `tempo` and `loki`. See [Grafana](#grafana).
- **Rate panels empty, stats show values**: not enough samples yet. Wait for a second 60-second export.
- **Recent traces is empty**: Tempo needs about 30 seconds. Also check the sampling ratio isn't set very low.
- **Logs panel is empty but traces work**: check the Serilog OTLP sink is enabled. It needs the same `Otlp:Endpoint` as traces. See [otlp-collector-api.md](otlp-collector-api.md#sending-from-the-api).
- **Board is gone after recreating the container**: the `-v otel-lgtm-data:/data` volume was missing. Import again, or provision it.
