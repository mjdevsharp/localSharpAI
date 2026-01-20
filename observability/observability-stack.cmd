@echo off
REM OpenTelemetry Stack Quick Start Script for Windows
REM Usage: observability-stack.cmd start|stop|logs|status

setlocal enabledelayedexpansion

if "%1"=="" goto help
if "%1"=="help" goto help
if "%1"=="--help" goto help
if "%1"=="-h" goto help

if "%1"=="start" (
    echo.
    echo ======================================
    echo Starting OpenTelemetry Stack
    echo ======================================
    cd /d "%~dp0"
    docker-compose up -d
    
    echo.
    echo Waiting for services to initialize...
    timeout /t 10 /nobreak
    
    echo.
    echo ======================================
    echo Services Status
    echo ======================================
    docker-compose ps
    
    echo.
    echo Stack is running!
    echo.
    echo Access the UIs:
    echo   Grafana:    http://localhost:3000 [admin/admin]
    echo   Jaeger:     http://localhost:16686
    echo   Prometheus: http://localhost:9090
    echo.
    echo Next steps:
    echo   1. Follow observability\SETUP.md to integrate with your application
    echo   2. Make API requests to generate telemetry
    echo   3. View traces in Jaeger and metrics in Grafana
    goto end
)

if "%1"=="stop" (
    echo.
    echo ======================================
    echo Stopping OpenTelemetry Stack
    echo ======================================
    cd /d "%~dp0"
    docker-compose down
    
    echo.
    echo Stack stopped successfully
    echo.
    echo To remove all data and volumes, run:
    echo   docker-compose down -v
    goto end
)

if "%1"=="logs" (
    if "%2"=="" (
        set SERVICE=otel-collector
    ) else (
        set SERVICE=%2
    )
    
    echo.
    echo ======================================
    echo Logs for: !SERVICE!
    echo ======================================
    cd /d "%~dp0"
    docker-compose logs -f !SERVICE!
    goto end
)

if "%1"=="status" (
    echo.
    echo ======================================
    echo OpenTelemetry Stack Status
    echo ======================================
    cd /d "%~dp0"
    docker-compose ps
    goto end
)

:help
echo.
echo OpenTelemetry Stack Management Script
echo.
echo Usage: observability-stack.cmd [COMMAND]
echo.
echo Commands:
echo   start       Start the observability stack
echo   stop        Stop the observability stack
echo   logs        Show logs from a service (default: otel-collector)
echo   status      Show status of services
echo   help        Show this help message
echo.
echo Examples:
echo   observability-stack.cmd start
echo   observability-stack.cmd logs jaeger
echo   observability-stack.cmd status
echo.
echo For more information, see:
echo   - observability\README.md
echo   - observability\SETUP.md
echo   - docs\observability.md
echo.

:end
endlocal
