# OTLP Collector API

The OpenTelemetry Collector (`otel-collector`) is the one entry point for telemetry. It speaks OTLP, the OpenTelemetry protocol, on two ports. Anything that can send OTLP can use it: the API, a script, `curl`, or another service. The pipeline behind it is in [opentelemetry.md](opentelemetry.md), and the config is `docs/observability/otel-collector.yaml`.

| Port | Transport | Base URL |
|---|---|---|
| `4317` | gRPC (HTTP/2, Protobuf) | `localhost:4317` |
| `4318` | HTTP/1.1 | `http://localhost:4318` |

Both ports are published to `127.0.0.1` only, so they are reachable from this machine but not from the network. They accept the same three signals and route them identically.

There are no other endpoints. The Collector has no query API: you send data in and read it back in Grafana or Seq. It has no health-check endpoint configured either (the `health_check` extension isn't in the config), so use the [smoke test](#smoke-test) to check it is up.

## What happens to data

```
POST :4318/v1/traces  ─┐
gRPC :4317 TraceService┘──► memory_limiter ─► batch ─┬─► otel-lgtm:4317 (Tempo)
                                                     └─► seq:80/ingest/otlp
POST :4318/v1/metrics ─┐
gRPC :4317 MetricsSvc ─┘──► memory_limiter ─► batch ───► otel-lgtm:4317 (Prometheus)

POST :4318/v1/logs    ─┐
gRPC :4317 LogsService ┘──► memory_limiter ─► batch ───► otel-lgtm:4317 (Loki)
```

| Signal | Goes to | Notes |
|---|---|---|
| Traces | Grafana (Tempo) and Seq | The only signal copied to two backends |
| Metrics | Grafana (Prometheus) | |
| Logs | Grafana (Loki) | Seq gets API logs from the Serilog Seq sink, not from here, so they aren't stored twice |

- **`memory_limiter`** refuses new data when the Collector nears 256 MiB, instead of crashing. A refused request comes back as an error, and well-behaved SDKs retry.
- **`batch`** holds data briefly and sends it in groups, so a successful response means "received", not "already visible in Grafana".

## HTTP API (port 4318)

One endpoint per signal. All use `POST`.

| Signal | Endpoint | Request body |
|---|---|---|
| Traces | `POST /v1/traces` | `ExportTraceServiceRequest` |
| Metrics | `POST /v1/metrics` | `ExportMetricsServiceRequest` |
| Logs | `POST /v1/logs` | `ExportLogsServiceRequest` |

### Headers

| Header | Value | Required |
|---|---|---|
| `Content-Type` | `application/json` or `application/x-protobuf` | Yes |
| `Content-Encoding` | `gzip` | No. Compresses the body |

### Responses

| Status | Meaning |
|---|---|
| `200 OK` | Accepted. The body is `{"partialSuccess":{}}` for JSON. If part of the data was rejected, `partialSuccess` says how much and why |
| `400 Bad Request` | The body can't be parsed. Don't retry unchanged |
| `404 Not Found` | Wrong path. Common mistake: posting to `/` or `/v1/trace` |
| `405 Method Not Allowed` | Used `GET`. Only `POST` is accepted |
| `415 Unsupported Media Type` | `Content-Type` is not one of the two above |
| `429` / `502` / `503` / `504` | Overloaded or unavailable. Retry with backoff |

### JSON encoding rules

These differ from ordinary Protobuf-JSON, and are the usual reason a hand-written payload fails:

- `traceId` is **32 hex characters** and `spanId` is **16 hex characters**. They are not base64.
- Time fields (`startTimeUnixNano`, `timeUnixNano`) are **strings** holding nanoseconds since the Unix epoch.
- Enums are **integers**, not names: span `kind` `2` is `SERVER`, status `code` `2` is `ERROR`.
- Field names are `lowerCamelCase`.
- Attribute values are typed: `{"stringValue": "x"}`, `{"intValue": "42"}`, `{"boolValue": true}`, `{"doubleValue": 1.5}`.

### Traces

```bash
curl -i http://localhost:4318/v1/traces \
  -H "Content-Type: application/json" \
  -d '{
    "resourceSpans": [{
      "resource": {
        "attributes": [
          { "key": "service.name", "value": { "stringValue": "curl-test" } }
        ]
      },
      "scopeSpans": [{
        "scope": { "name": "manual" },
        "spans": [{
          "traceId": "5b8efff798038103d269b633813fc60c",
          "spanId": "eee19b7ec3c1b174",
          "name": "manual-span",
          "kind": 1,
          "startTimeUnixNano": "1760000000000000000",
          "endTimeUnixNano": "1760000000250000000",
          "attributes": [
            { "key": "demo", "value": { "stringValue": "yes" } }
          ],
          "status": { "code": 1 }
        }]
      }]
    }]
  }'
```

Span `kind`: `1` internal, `2` server, `3` client, `4` producer, `5` consumer. Status `code`: `0` unset, `1` ok, `2` error.

Replace the two timestamps with a current time, or the trace appears at the wrong place on the time axis. Get one with `date +%s%N` in bash.

### Metrics

```bash
curl -i http://localhost:4318/v1/metrics \
  -H "Content-Type: application/json" \
  -d '{
    "resourceMetrics": [{
      "resource": {
        "attributes": [
          { "key": "service.name", "value": { "stringValue": "curl-test" } }
        ]
      },
      "scopeMetrics": [{
        "scope": { "name": "manual" },
        "metrics": [{
          "name": "demo.requests",
          "unit": "1",
          "sum": {
            "aggregationTemporality": 2,
            "isMonotonic": true,
            "dataPoints": [{
              "asInt": "5",
              "startTimeUnixNano": "1760000000000000000",
              "timeUnixNano": "1760000060000000000",
              "attributes": [
                { "key": "result", "value": { "stringValue": "ok" } }
              ]
            }]
          }
        }]
      }]
    }]
  }'
```

`aggregationTemporality`: `1` delta, `2` cumulative. The .NET SDK sends cumulative. In Prometheus this arrives as `demo_requests_total`.

### Logs

```bash
curl -i http://localhost:4318/v1/logs \
  -H "Content-Type: application/json" \
  -d '{
    "resourceLogs": [{
      "resource": {
        "attributes": [
          { "key": "service.name", "value": { "stringValue": "curl-test" } }
        ]
      },
      "scopeLogs": [{
        "scope": { "name": "manual" },
        "logRecords": [{
          "timeUnixNano": "1760000000000000000",
          "severityNumber": 9,
          "severityText": "INFO",
          "body": { "stringValue": "hello from curl" },
          "traceId": "5b8efff798038103d269b633813fc60c",
          "spanId": "eee19b7ec3c1b174"
        }]
      }]
    }]
  }'
```

`severityNumber`: `5` debug, `9` info, `13` warn, `17` error, `21` fatal. Putting a `traceId` and `spanId` on the record is what links the log to its trace in Grafana.

### From PowerShell

On Windows PowerShell, `curl` is an alias for `Invoke-WebRequest`, which has different flags. Use `curl.exe`, or:

```powershell
$body = '{"resourceSpans":[]}'
Invoke-RestMethod -Method Post -Uri http://localhost:4318/v1/traces `
  -ContentType 'application/json' -Body $body
```

## gRPC API (port 4317)

Three unary RPCs, one per signal. The Protobuf definitions are in the [opentelemetry-proto](https://github.com/open-telemetry/opentelemetry-proto) repository.

| Signal | Service and method |
|---|---|
| Traces | `opentelemetry.proto.collector.trace.v1.TraceService/Export` |
| Metrics | `opentelemetry.proto.collector.metrics.v1.MetricsService/Export` |
| Logs | `opentelemetry.proto.collector.logs.v1.LogsService/Export` |

Each takes the same request message as its HTTP twin and returns an `Export…ServiceResponse` with an optional `partial_success`. This port is plain-text (no TLS), so clients must use an `http://` endpoint or an "insecure" option.

Calling it by hand with [`grpcurl`](https://github.com/fullstorydev/grpcurl), using the proto files:

```bash
grpcurl -plaintext -proto opentelemetry/proto/collector/trace/v1/trace_service.proto \
  -import-path ./opentelemetry-proto \
  -d '{"resourceSpans":[]}' \
  localhost:4317 opentelemetry.proto.collector.trace.v1.TraceService/Export
```

The Collector does not enable gRPC server reflection, so `grpcurl list` does not work without the proto files.

### gRPC or HTTP?

| | gRPC `4317` | HTTP `4318` |
|---|---|---|
| Format | Protobuf only | Protobuf or JSON |
| Hand-testing | Needs `grpcurl` and proto files | Works with `curl` |
| Through proxies and firewalls | Needs HTTP/2 | Works anywhere HTTP does |
| Overhead | Lower, with streaming connections | Slightly higher |

Use gRPC for services, as the API does by default. Use HTTP for scripts, debugging and anything behind an HTTP proxy.

## Sending from the API

The API picks the port from `OpenTelemetry:Otlp:Protocol`. It never needs the per-signal path in config.

| `Protocol` | `Endpoint` | Traces and metrics go to | Logs go to |
|---|---|---|---|
| `grpc` | `http://localhost:4317` | the gRPC services above | the gRPC `LogsService` |
| `http` | `http://localhost:4318` | `/v1/traces` and `/v1/metrics`, appended by `OpenTelemetryExtensions` | `/v1/logs`, appended by the Serilog OTLP sink |

Don't put `/v1/traces` in the `Endpoint`. The code adds it, so it would be doubled. A blank `Endpoint` turns all export off.

The API tags every signal with the same resource attributes, which is how Grafana lines them up:

| Attribute | Value |
|---|---|
| `service.name` | `OpenTelemetry:ServiceName`, `ShopAPI` by default |
| `service.version` | `OpenTelemetry:ServiceVersion` |
| `service.instance.id` | The machine name |
| `deployment.environment.name` | The ASP.NET environment, lowercased (`development`) |

### Other clients

Any OpenTelemetry SDK can use the same Collector with the standard environment variables:

```bash
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc            # or http/protobuf with port 4318
OTEL_SERVICE_NAME=my-service
```

With `http/protobuf`, SDKs add `/v1/traces`, `/v1/metrics` and `/v1/logs` to the base URL themselves. The .NET exporter is the exception, because it only does this for environment variables, which is why the API adds the path itself.

## Smoke test

Use this to check the Collector is up and listening, since it has no health endpoint. An empty request is valid:

```bash
curl -i -X POST http://localhost:4318/v1/traces \
  -H "Content-Type: application/json" -d '{"resourceSpans":[]}'
```

Expected: `HTTP/1.1 200 OK` and `{"partialSuccess":{}}`.

To check the whole path, send the trace example above and find it in Grafana:

1. Open **Explore → Tempo**.
2. Run `{ resource.service.name = "curl-test" }`.
3. The trace shows up after about 30 seconds.

There is nothing to clean up. The test data is a few records and ages out with the rest.

## Troubleshooting

- **`Connection refused` on 4317 or 4318**: the Collector isn't running. Check `docker ps`, then `docker start otel-collector`.
- **`404` on 4318**: the path is wrong. It must be `/v1/traces`, `/v1/metrics` or `/v1/logs`.
- **`400` with a JSON payload**: check the [encoding rules](#json-encoding-rules). IDs must be hex, times must be strings, and enums must be numbers.
- **`200` but nothing in Grafana**: wait about 30 seconds for traces and 2 minutes for rate panels. If still empty, read `docker logs otel-collector` for export errors toward `otel-lgtm` or `seq`.
- **Collector logs `no such host` for `otel-lgtm` or `seq`**: the containers aren't on the `shop-telemetry` network. See [opentelemetry.md](opentelemetry.md#run-the-dashboards).
- **Port already allocated**: the Aspire dashboard from the lab uses the same host ports. Run one at a time.
- **From the API, nothing arrives and no error shows**: check whether `HTTP_PROXY` is set. .NET sends the exporter's traffic to the proxy. Set `NO_PROXY=localhost,127.0.0.1`.
- **Data accepted but a custom metric is missing**: this is an API-side problem, not a Collector one. Check the meter name is in `AddMeter`.
