# 🎯 OpenTelemetry Setup - Quick Reference Guide

## 📁 Project Structure

```
LocalSharpAI/
│
├── docs/
│   ├── observability.md                    ⭐ Architecture & Concepts
│   ├── observability-azure-deployment.md   ⭐ Production Guide
│   └── ...
│
├── observability/                          ⭐ NEW - Complete OTel Stack
│   ├── docker-compose.yml                  ← Start here: docker-compose up
│   ├── README.md                           ← Quick overview
│   ├── SETUP.md                            ⭐ READ FIRST - Integration steps
│   ├── CUSTOM_METRICS.md                   ← Add custom metrics
│   ├── ROADMAP.md                          ← Implementation timeline
│   ├── .env.example                        ← Configuration template
│   │
│   ├── otel-collector/
│   │   └── otel-collector-config.yml       ← Receives & exports telemetry
│   ├── prometheus/
│   │   ├── prometheus.yml                  ← Scrape config
│   │   └── alert_rules.yml                 ← 11 pre-built alerts
│   ├── loki/
│   │   └── loki-config.yml                 ← Log ingestion
│   └── grafana/
│       ├── provisioning/                   ← Auto-configure datasources
│       └── dashboards/                     ← 3 pre-built dashboards
│
├── src/
│   ├── LocalSharpAi.Api/                   ← Needs OTel integration
│   │   └── Program.cs                      ← Add OTel here
│   └── ...
│
├── OBSERVABILITY_SUMMARY.md                ← What was created
├── OBSERVABILITY_SETUP_COMPLETE.md         ← This guide
└── README.md
```

## 🚀 Getting Started (3 Steps)

### Step 1: Start Observability Stack (2 minutes)
```powershell
cd observability
docker-compose up -d

# Verify (should see 5 services as "Up")
docker-compose ps
```

### Step 2: Access the UIs
| Service | URL | Default Credentials |
|---------|-----|---------------------|
| Grafana | http://localhost:3000 | admin / admin |
| Jaeger | http://localhost:16686 | - |
| Prometheus | http://localhost:9090 | - |

### Step 3: Integrate with Application
👉 **Follow** `observability/SETUP.md` (10 minutes)

## 📚 Documentation Roadmap

```
               START HERE
                   ↓
        observability/README.md
        (5 min read - overview)
                   ↓
        observability/SETUP.md
        (10 min read - integration steps)
                   ↓
        Choose your path:
        
    ├─→ Want custom metrics?
    │   └─→ observability/CUSTOM_METRICS.md
    │
    ├─→ Understanding architecture?
    │   └─→ docs/observability.md
    │
    └─→ Going to production?
        └─→ docs/observability-azure-deployment.md
```

## ⚡ 5-Minute Quick Start Checklist

- [ ] Navigate to observability folder: `cd observability`
- [ ] Start stack: `docker-compose up -d`
- [ ] Wait 30 seconds for services to start
- [ ] Verify all 5 containers running: `docker-compose ps`
- [ ] Open Grafana: http://localhost:3000 (login: admin/admin)
- [ ] Verify datasources are configured (green lights)
- [ ] View pre-built dashboards

## 🎯 Integration Checklist (SETUP.md)

```
Phase 1: NuGet Packages (5 min)
├─ [ ] Add OpenTelemetry
├─ [ ] Add OpenTelemetry.Exporter.OpenTelemetryProtocol
├─ [ ] Add instrumentation packages (AspNetCore, Http, EF Core, etc.)
└─ [ ] Restore packages: dotnet restore

Phase 2: Program.cs Configuration (10 min)
├─ [ ] Add using statements
├─ [ ] Configure OTel tracing
├─ [ ] Configure OTel metrics
├─ [ ] Configure OTel logging
└─ [ ] Add OTLP exporter

Phase 3: Environment Variables (3 min)
├─ [ ] Copy .env.example to .env
├─ [ ] Set OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
└─ [ ] Review other OTel variables

Phase 4: Testing (10 min)
├─ [ ] Start API: dotnet run
├─ [ ] Make test request: curl http://localhost:5000/api/v1/...
├─ [ ] Check Jaeger for trace: http://localhost:16686
├─ [ ] Check Prometheus for metrics: http://localhost:9090
└─ [ ] Check Grafana dashboard for data
```

## 📊 What Each Component Does

| Component | Port | Purpose | Access |
|-----------|------|---------|--------|
| **OTel Collector** | 4317 | Receives telemetry from app | N/A (internal) |
| **Jaeger** | 16686 | Distributed tracing | UI + Grafana |
| **Prometheus** | 9090 | Metrics storage | UI + Grafana |
| **Loki** | 3100 | Log aggregation | Grafana only |
| **Grafana** | 3000 | Visualization | UI |

## 🔍 View Your Telemetry

### 1. Traces (Jaeger)
```
http://localhost:16686
→ Service: LocalSharpAi.Api
→ Operation: Any
→ Find Traces
```

### 2. Metrics (Prometheus)
```
http://localhost:9090
→ Query: http_request_duration_seconds
→ Execute
```

### 3. Logs (Grafana → Loki)
```
http://localhost:3000
→ Explore
→ Data source: Loki
→ Query: {job="localsharpai-api"}
```

### 4. Dashboards (Grafana)
```
http://localhost:3000
→ Dashboards
→ View: API Performance, Logs, Traces
```

## ⚠️ Troubleshooting Quick Fixes

**Services won't start?**
```powershell
# Check if ports are in use
netstat -ano | findstr ":3000"  # Grafana
netstat -ano | findstr ":9090"  # Prometheus

# Free port (Windows PowerShell)
Stop-Process -Id <PID> -Force
```

**No data in Jaeger?**
- Verify endpoint: `OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317`
- Check collector: `docker logs otel-collector`
- Make sure Program.cs has OTel configuration

**Grafana datasources showing red?**
- Wait 30 seconds for services to initialize
- Verify service names match in datasource URLs
- Refresh page (F5)

**See detailed troubleshooting**: `observability/SETUP.md#troubleshooting`

## 🎯 Key URLs

```
Grafana:            http://localhost:3000     admin/admin
Jaeger UI:          http://localhost:16686
Prometheus:         http://localhost:9090
OTel Collector:     localhost:4317 (gRPC)
Prometheus Scrape:  http://localhost:8889/metrics
```

## 📖 Key Files Reference

| File | Lines | Purpose |
|------|-------|---------|
| `SETUP.md` | 400+ | Step-by-step integration guide |
| `docker-compose.yml` | 110 | All services configuration |
| `otel-collector-config.yml` | 80+ | Receiver/processor/exporter config |
| `prometheus/alert_rules.yml` | 100+ | Pre-built alert rules |
| `docs/observability.md` | 500+ | Architecture deep dive |
| `CUSTOM_METRICS.md` | 200+ | Code examples for metrics |

## 🔐 Security Notes

✅ **Local Development**
- All data stays on your machine
- No external communication
- Perfect for dev/test

⚠️ **Production**
- Use Azure Application Insights
- Implement encryption
- Set access controls
- Configure retention policies

See `docs/observability-azure-deployment.md` for production setup.

## 📊 Pre-built Features Ready to Use

### Dashboards (3)
✅ API Performance Dashboard
✅ Logs Dashboard  
✅ Traces Dashboard

### Alert Rules (11)
✅ High error rate
✅ High latency
✅ Service down
✅ High memory
✅ Agent failures
✅ Model load failures
✅ And more...

### Custom Metrics (Templates Ready)
✅ Agent execution tracking
✅ Model performance
✅ Tool invocations
✅ Business metrics

## 🚀 Typical Workflow

```
1. Start stack (30s)
   docker-compose up -d

2. Integrate app (30 min)
   - Add NuGet packages
   - Update Program.cs
   - Set environment vars
   - Test locally

3. Monitor (ongoing)
   - Check Grafana dashboards
   - Review alerts
   - Analyze traces
   - Query metrics

4. Extend (optional)
   - Add custom metrics
   - Create dashboards
   - Adjust alerts
   - Plan production
```

## 💡 Quick Tips

**Tip 1**: Start with `observability/SETUP.md` for complete walkthrough
**Tip 2**: Use Grafana as your main UI (everything in one place)
**Tip 3**: Jaeger is best for debugging slow/failed requests
**Tip 4**: Prometheus is best for trends and alerting
**Tip 5**: Loki is best for error log analysis

## ✅ Success Criteria

You'll know it's working when:
- ✅ All 5 docker services show "Up"
- ✅ Grafana accessible at http://localhost:3000
- ✅ Datasources showing green in Grafana
- ✅ Dashboards showing data
- ✅ Can see traces in Jaeger
- ✅ Can see metrics in Prometheus
- ✅ Can see logs in Loki/Grafana

## 📞 Get Help

**For integration issues**: See `observability/SETUP.md#troubleshooting`
**For architecture questions**: See `docs/observability.md`
**For custom metrics**: See `observability/CUSTOM_METRICS.md`
**For production**: See `docs/observability-azure-deployment.md`

---

## 🎬 Your Next Step

👉 **Open and read**: `observability/SETUP.md`

It will walk you through every integration step with code examples.

**Time needed**: 30 minutes total (10 min to read + 20 min to implement)

---

**Good luck!** 🚀 Your observability stack is ready to go.
