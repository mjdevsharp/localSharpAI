#!/usr/bin/env bash
# OpenTelemetry Stack Quick Start Script (Optional)
# Makes it easier to manage the observability stack

set -e

SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
cd "$SCRIPT_DIR"

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

function print_header() {
    echo -e "${BLUE}========================================${NC}"
    echo -e "${BLUE}$1${NC}"
    echo -e "${BLUE}========================================${NC}"
}

function print_success() {
    echo -e "${GREEN}✓ $1${NC}"
}

function print_warning() {
    echo -e "${YELLOW}⚠ $1${NC}"
}

# Check if Docker is running
check_docker() {
    if ! docker ps &> /dev/null; then
        echo "Docker is not running. Please start Docker and try again."
        exit 1
    fi
    print_success "Docker is running"
}

# Start the stack
start_stack() {
    print_header "Starting OpenTelemetry Stack"
    
    cd observability
    docker-compose up -d
    
    print_success "Stack starting..."
    print_warning "Waiting for services to initialize..."
    sleep 10
    
    echo ""
    print_header "Services Status"
    docker-compose ps
    
    echo ""
    print_success "Stack is running!"
    echo ""
    echo -e "${GREEN}Access the UIs:${NC}"
    echo "  Grafana:    ${BLUE}http://localhost:3000${NC} (admin/admin)"
    echo "  Jaeger:     ${BLUE}http://localhost:16686${NC}"
    echo "  Prometheus: ${BLUE}http://localhost:9090${NC}"
    echo ""
    echo -e "${YELLOW}Next steps:${NC}"
    echo "  1. Follow observability/SETUP.md to integrate with your application"
    echo "  2. Make API requests to generate telemetry"
    echo "  3. View traces in Jaeger and metrics in Grafana"
}

# Stop the stack
stop_stack() {
    print_header "Stopping OpenTelemetry Stack"
    
    cd observability
    docker-compose down
    
    print_success "Stack stopped"
    echo ""
    echo "To remove all data and volumes, run:"
    echo "  docker-compose down -v"
}

# Show logs
show_logs() {
    SERVICE=${1:-"otel-collector"}
    
    print_header "Logs for: $SERVICE"
    
    cd observability
    docker-compose logs -f "$SERVICE"
}

# Show help
show_help() {
    cat << EOF
OpenTelemetry Stack Management Script

Usage: ./observability-stack.sh [COMMAND]

Commands:
  start       Start the observability stack
  stop        Stop the observability stack
  logs        Show logs from a service (default: otel-collector)
  status      Show status of services
  help        Show this help message

Examples:
  ./observability-stack.sh start
  ./observability-stack.sh logs jaeger
  ./observability-stack.sh status

For more information, see:
  - observability/README.md
  - observability/SETUP.md
  - docs/observability.md

EOF
}

# Show status
show_status() {
    print_header "OpenTelemetry Stack Status"
    
    cd observability
    
    if docker-compose ps | grep -q "Up"; then
        print_success "Stack is running"
        echo ""
        docker-compose ps
    else
        print_warning "Stack is not running"
        echo "To start the stack, run: ./observability-stack.sh start"
    fi
}

# Main script logic
case "${1:-help}" in
    start)
        check_docker
        start_stack
        ;;
    stop)
        stop_stack
        ;;
    logs)
        check_docker
        show_logs "$2"
        ;;
    status)
        check_docker
        show_status
        ;;
    help|--help|-h)
        show_help
        ;;
    *)
        echo "Unknown command: $1"
        echo ""
        show_help
        exit 1
        ;;
esac
