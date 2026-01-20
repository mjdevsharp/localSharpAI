# Custom Metrics Guide for LocalSharpAI

This guide shows how to add custom metrics for LocalSharpAI-specific operations.

## Overview

OpenTelemetry allows custom metrics via Meters and Instruments. This document covers adding metrics for:
- Agent execution
- Model operations
- Tool invocations
- Business metrics

## Metrics Types

### 1. Counter (monotonically increasing)
```csharp
var meter = new Meter("LocalSharpAi.Metrics");
var agentExecutionCounter = meter.CreateCounter<long>("agent_execution_total");

// Usage
agentExecutionCounter.Add(1, new KeyValuePair<string, object?>("agent_id", agentId));
```

**Use for**: Request counts, error counts, total invocations

### 2. Histogram (distribution)
```csharp
var agentLatencyHistogram = meter.CreateHistogram<double>("agent_execution_duration_seconds");

// Usage
agentLatencyHistogram.Record(executionTime.TotalSeconds);
```

**Use for**: Latencies, sizes, durations

### 3. ObservableGauge (snapshot value)
```csharp
var activeAgentsGauge = meter.CreateObservableGauge(
    "agent_active_count",
    () => new Measurement<long>(activeAgents.Count)
);
```

**Use for**: Current values, gauges, states

## Example: Agent Execution Metrics

### Code Implementation

```csharp
// In your agent execution service
public class AgentExecutionService
{
    private readonly Meter _meter;
    private readonly Counter<long> _executionCounter;
    private readonly Histogram<double> _durationHistogram;
    private readonly Counter<long> _errorCounter;

    public AgentExecutionService()
    {
        _meter = new Meter("LocalSharpAi.Agent");
        _executionCounter = _meter.CreateCounter<long>("agent_execution_total");
        _durationHistogram = _meter.CreateHistogram<double>("agent_execution_duration_seconds");
        _errorCounter = _meter.CreateCounter<long>("agent_errors_total");
    }

    public async Task<AgentResult> ExecuteAsync(string agentId, AgentRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await RunAgentLogic(agentId, request);
            
            _executionCounter.Add(1, new KeyValuePair<string, object?>("agent_id", agentId));
            _durationHistogram.Record(stopwatch.Elapsed.TotalSeconds);
            
            return result;
        }
        catch (Exception ex)
        {
            _errorCounter.Add(1, new KeyValuePair<string, object?>("agent_id", agentId));
            throw;
        }
    }
}
```

### Prometheus Queries

```promql
# Agent execution rate (requests per second)
rate(agent_execution_total[5m])

# Agent error rate
rate(agent_errors_total[5m])

# Agent execution time percentiles
histogram_quantile(0.99, agent_execution_duration_seconds_bucket)
histogram_quantile(0.95, agent_execution_duration_seconds_bucket)

# Agent-specific metrics
agent_execution_total{agent_id="my-agent"}
rate(agent_errors_total{agent_id="my-agent"}[5m])
```

## Example: Model Runtime Metrics

### Code Implementation

```csharp
public class ModelRuntimeService
{
    private readonly Meter _meter;
    private readonly Histogram<double> _modelLoadDuration;
    private readonly Counter<long> _modelLoadErrors;
    private readonly Histogram<double> _inferenceDuration;
    private readonly Counter<long> _tokensGenerated;

    public ModelRuntimeService()
    {
        _meter = new Meter("LocalSharpAi.ModelRuntime");
        _modelLoadDuration = _meter.CreateHistogram<double>("model_load_duration_seconds");
        _modelLoadErrors = _meter.CreateCounter<long>("model_load_errors_total");
        _inferenceDuration = _meter.CreateHistogram<double>("inference_duration_seconds");
        _tokensGenerated = _meter.CreateCounter<long>("tokens_generated_total");
    }

    public async Task<string> LoadModelAsync(string modelPath)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var model = await LoadModelLogic(modelPath);
            _modelLoadDuration.Record(stopwatch.Elapsed.TotalSeconds);
            return model;
        }
        catch (Exception ex)
        {
            _modelLoadErrors.Add(1, new KeyValuePair<string, object?>("model", modelPath));
            throw;
        }
    }

    public async Task<InferenceResult> InferAsync(string model, string prompt)
    {
        var stopwatch = Stopwatch.StartNew();
        
        var result = await InferenceLogic(model, prompt);
        
        _inferenceDuration.Record(stopwatch.Elapsed.TotalSeconds);
        _tokensGenerated.Add(result.TokenCount);
        
        return result;
    }
}
```

### Prometheus Queries

```promql
# Model load latency
histogram_quantile(0.95, model_load_duration_seconds_bucket)

# Model load failures per minute
rate(model_load_errors_total[1m])

# Inference latency
histogram_quantile(0.99, inference_duration_seconds_bucket)

# Total tokens generated
rate(tokens_generated_total[5m])
```

## Example: Tool Invocation Metrics

```csharp
public class ToolRegistry
{
    private readonly Meter _meter;
    private readonly Counter<long> _toolInvocationCounter;
    private readonly Histogram<double> _toolDurationHistogram;
    private readonly Counter<long> _toolErrorCounter;

    public ToolRegistry()
    {
        _meter = new Meter("LocalSharpAi.Tools");
        _toolInvocationCounter = _meter.CreateCounter<long>("tool_invocation_total");
        _toolDurationHistogram = _meter.CreateHistogram<double>("tool_duration_seconds");
        _toolErrorCounter = _meter.CreateCounter<long>("tool_errors_total");
    }

    public async Task<object> InvokeAsync(string toolName, Dictionary<string, object> args)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            var result = await ExecuteTool(toolName, args);
            
            _toolInvocationCounter.Add(1, 
                new KeyValuePair<string, object?>("tool", toolName),
                new KeyValuePair<string, object?>("status", "success"));
            
            _toolDurationHistogram.Record(stopwatch.Elapsed.TotalSeconds);
            
            return result;
        }
        catch (Exception ex)
        {
            _toolErrorCounter.Add(1, new KeyValuePair<string, object?>("tool", toolName));
            throw;
        }
    }
}
```

## Registering Custom Metrics in Program.cs

```csharp
// In Program.cs
builder.Services
    .AddOpenTelemetry()
    .WithMetrics(meterProvider => meterProvider
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddMeter("LocalSharpAi.Metrics")           // Your custom meter
        .AddMeter("LocalSharpAi.Agent")
        .AddMeter("LocalSharpAi.ModelRuntime")
        .AddMeter("LocalSharpAi.Tools")
        .AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri("http://localhost:4317");
        }));
```

## Grafana Dashboard Queries

### Agent Metrics Panel

```json
{
  "targets": [
    {
      "expr": "rate(agent_execution_total[5m])",
      "legendFormat": "{{agent_id}}"
    }
  ]
}
```

### Model Performance Panel

```json
{
  "targets": [
    {
      "expr": "histogram_quantile(0.99, inference_duration_seconds_bucket)",
      "legendFormat": "p99 inference latency"
    }
  ]
}
```

## Best Practices

✅ **DO**:
- Use lowercase metric names with underscores
- Include unit suffixes (`_seconds`, `_bytes`, `_total`)
- Add relevant dimensions (agent_id, model, tool, etc.)
- Record histogram data for latency metrics
- Use counters for cumulative values

❌ **DON'T**:
- Create high-cardinality metrics (unbounded label values)
- Mix units in the same metric
- Record very large numbers without aggregation
- Create too many distinct metrics (keep < 100)

## Testing Metrics Locally

```bash
# 1. Start the observability stack
cd observability
docker-compose up -d

# 2. Start the API
cd src/LocalSharpAi.Api
dotnet run

# 3. Make requests to generate metrics
curl http://localhost:5000/api/v1/agents/execute -X POST -H "Content-Type: application/json" -d '{"agentId": "test-agent"}'

# 4. Query Prometheus
# Open http://localhost:9090
# Search for your metric name (e.g., agent_execution_total)

# 5. View in Grafana
# Open http://localhost:3000
# Create a dashboard panel querying your metrics
```

## References

- [OpenTelemetry Metrics API](https://opentelemetry.io/docs/reference/specification/metrics/api/)
- [Prometheus Metric Types](https://prometheus.io/docs/concepts/metric_types/)
- [PromQL Query Examples](https://prometheus.io/docs/prometheus/latest/querying/examples/)
