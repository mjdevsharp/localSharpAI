# OpenTelemetry Local Stack Setup Guide

## Quick Start (2 minutes)

### 1. Start the Stack

```bash
cd observability
docker-compose up -d
```

Wait for all services to be healthy (30-60 seconds):
```bash
docker-compose ps
```

Expected output (all should show `(healthy)`):
```
NAME                COMMAND                  SERVICE             STATUS                   PORTS
jaeger              "/go/bin/all-in-one-…"   jaeger              Up X minutes (healthy)   ...
loki                "/loki -config.file=…"   loki                Up X minutes (healthy)   ...
otel-collector      "--config=/etc/otel-…"   otel-collector      Up X minutes (healthy)   ...
prometheus          "/bin/prometheus --c…"   prometheus          Up X minutes (healthy)   ...
grafana             "/run.sh"                grafana             Up X minutes (healthy)   ...
```

### 2. Verify Datasources in Grafana

1. Open http://localhost:3000 (login: admin/admin)
2. Go to **Connections → Data Sources**
3. Verify all 3 datasources are connected:
   - ✅ Prometheus (http://prometheus:9090) - marked as default
   - ✅ Jaeger (http://jaeger:16686)
   - ✅ Loki (http://loki:3100)

### 3. Access the UIs

| Service | URL | Credentials |
|---------|-----|-------------|
| Grafana | http://localhost:3000 | admin / admin |
| Jaeger | http://localhost:16686 | - |
| Prometheus | http://localhost:9090 | - |

## API Configuration

### Update appsettings.json

```json
{
  "OpenTelemetry": {
    "Enabled": true,
    "ExporterEndpoint": "http://localhost:4317",
    "ServiceName": "LocalSharpAi.Api",
    "ServiceVersion": "1.0.0",
    "Environment": "development",
    "SamplingRate": 1.0
  }
}
```

### Update Program.cs

Add OpenTelemetry instrumentation:

```csharp
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using OpenTelemetry.Logs;
using OpenTelemetry.Exporter;

// ... existing code ...

var builder = WebApplication.CreateBuilder(args);

// Add OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProvider => tracerProvider
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }))
    .WithMetrics(meterProvider => meterProvider
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }))
    .WithLogging(loggerProvider => loggerProvider
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }));

// Add Logging
builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;
});

// ... rest of configuration ...

var app = builder.Build();

// ... rest of middleware configuration ...

app.Run();
```

### Required NuGet Packages

```bash
cd src/LocalSharpAi.Api

# Core OpenTelemetry
dotnet add package OpenTelemetry
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol
dotnet add package OpenTelemetry.Extensions.Hosting

# Instrumentations
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
dotnet add package OpenTelemetry.Instrumentation.Http
dotnet add package OpenTelemetry.Instrumentation.EntityFrameworkCore
dotnet add package OpenTelemetry.Instrumentation.Runtime

# Optional: For advanced tracing
dotnet add package OpenTelemetry.Instrumentation.SqlClient
dotnet add package OpenTelemetry.Instrumentation.GrpcNetClient
```

## Verification

### 1. Start the API

```bash
cd src/LocalSharpAi.Api
dotnet run
```

### 2. Generate Telemetry

```bash
# Make some requests
curl http://localhost:5000/api/v1/health
curl http://localhost:5000/api/v1/some-endpoint
```

### 3. Check Jaeger (Traces)

1. Open http://localhost:16686
2. Select service: `LocalSharpAi.Api`
3. Click "Find Traces"
4. You should see your requests

### 4. Check Prometheus (Metrics)

1. Open http://localhost:9090
2. Search for: `http_request_duration_seconds`
3. Click "Execute"
4. You should see metrics appearing

### 5. Check Loki (Logs)

1. Open http://localhost:3000 (Grafana)
2. Go to Explore
3. Select "Loki" datasource
4. Query: `{job="localsharpai-api"}`
5. You should see logs

### 6. Check Grafana Dashboards

1. Open http://localhost:3000
2. Login: admin / admin
3. Go to Dashboards
4. Open "LocalSharpAI API Performance"
5. You should see metrics and charts

## Troubleshooting

### No telemetry appearing in Jaeger

**Check 1**: Verify OTel exporter endpoint
```bash
# In Program.cs, verify:
options.Endpoint = new Uri("http://localhost:4317");
```

**Check 2**: Verify OTel Collector is running
```bash
docker logs otel-collector
```

**Check 3**: Check logs for errors
```bash
curl http://localhost:5000/health 2>&1 | grep -i "telemetry\|otel"
```

### Metrics not showing in Prometheus

**Check 1**: Verify metrics endpoint is exposed
```bash
curl http://localhost:5000/metrics
```

**Check 2**: Check Prometheus scrape targets
- Open http://localhost:9090/targets
- Verify `localsharpai-api` target is "Up"

**Check 3**: Check OTel Collector Prometheus exporter
```bash
curl http://localhost:8889/metrics
```

### Logs not appearing in Loki

**Check 1**: Verify OTel Collector is receiving logs
```bash
docker logs otel-collector | grep -i "log\|otlp"
```

**Check 2**: Check Loki is healthy
```bash
curl http://localhost:3100/ready
```

**Check 3**: Verify logging is configured in Program.cs
```csharp
builder.Logging.AddOpenTelemetry(...);
```

## Stopping the Stack

```bash
cd observability
docker-compose down

# Remove volumes (keep data)
docker-compose down -v
```

## Performance Tuning

### OTel Collector Memory

Edit `docker-compose.yml`:
```yaml
otel-collector:
  environment:
    - GOGC=80  # Lower for less memory, higher for less GC
```

### Sampling Rate

In Program.cs:
```csharp
.WithTracing(tracerProvider => tracerProvider
    .SetSampler(new ProbabilitySampler(0.1))  // 10% sampling
```

### Batch Processor

In `otel-collector-config.yml`:
```yaml
processors:
  batch:
    send_batch_size: 256      # Larger = lower latency, higher memory
    timeout: 10s              # Longer = better throughput
```

## Data Retention

### Prometheus
Default: 15 days
Edit `docker-compose.yml`:
```yaml
prometheus:
  command:
    - '--storage.tsdb.retention.time=30d'
```

### Loki
Default: 7 days
Edit `loki/loki-config.yml`:
```yaml
limits_config:
  retention_period: 720h  # 30 days
```

### Jaeger
Default: 72 hours
Edit `docker-compose.yml`:
```yaml
jaeger:
  environment:
    - COLLECTOR_ZIPKIN_HOST_PORT=:9411
    - MEMORY_MAX_TRACES=10000
```

## Known Issues & Fixes (Phase 1)

### Issue 1: Port 9411 Already in Use
**Symptom**: `Bind for 0.0.0.0:9411 failed: port is already allocated`
**Fix**: Zipkin receiver has been disabled (using Jaeger instead)
**Status**: ✅ Fixed

### Issue 2: OTel Collector Configuration Errors
**Symptom**: `unknown type: "jaeger" for id: "jaeger"` or `spanmetrics` errors
**Fixes Applied**:
- Removed `otlp` exporter (not available in contrib image)
- Fixed `spanmetrics` processor dimensions syntax
- Fixed `resource` processor structure
**Status**: ✅ Fixed

### Issue 3: Loki Configuration Errors
**Symptom**: `field shared_store not found` or `auth not found`
**Fixes Applied**:
- Changed from deprecated `boltdb-shipper` to `tsdb` storage
- Removed invalid fields: `chunk_size_target`, `max_concurrent_pushes`, `enforce_metric_name`
- Changed `auth:` to `auth_enabled:`
**Status**: ✅ Fixed

### Issue 4: Datasources Not Appearing in Grafana
**Symptom**: No datasources in Connections → Data Sources
**Fix**: Moved datasources.yml to correct path: `grafana/provisioning/datasources/datasources.yml`
**Status**: ✅ Fixed

### Issue 5: Loki Returns 404 at Root URL
**Symptom**: `http://localhost:3100` returns 404
**Explanation**: This is normal! Loki has no web UI. Access via:
- Grafana Explore → Loki datasource
- API endpoint: `http://localhost:3100/ready` (returns "Ingester not ready" then "ready")
**Status**: ✅ Expected behavior

## Port Mapping Note

**External → Internal Port Mapping:**
- Jaeger: 14269 → 14268 (external port changed to avoid conflict)
- OTel Collector: 4317-4318, 8888-8889, 13133, 14269
- Prometheus: 9090
- Loki: 3100
- Grafana: 3000

This mapping is configured in `docker-compose.yml` and works correctly.

## Next Steps

1. **Proceed to Phase 2**: [Add custom metrics in your agents and models](#)
2. **Create dashboards** for your specific use cases
3. **Configure alerts** for critical scenarios
4. **Set up log aggregation** for debugging
5. **Integrate with Azure** for production (see observability-azure-deployment.md)
