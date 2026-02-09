#!/usr/bin/env bash
set -euo pipefail

# ============================================================================
# Restate .NET SDK Throughput Benchmark Runner
#
# Orchestrates: Restate server (Docker) -> .NET service -> benchmark client
#
# Usage:
#   ./benchmark-runner.sh [--requests N] [--parallel N] [--sequential]
#
# Prerequisites:
#   - Docker running
#   - .NET 10 SDK installed
#   - Ports 8080, 9090 available
# ============================================================================

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_DIR="$SCRIPT_DIR/throughput-benchmark"
RESTATE_IMAGE="${RESTATE_IMAGE:-docker.io/restatedev/restate:latest}"
RESTATE_CONTAINER="restate-benchmark"
SERVICE_PORT=9090
RESTATE_INGRESS_PORT=8080
RESTATE_ADMIN_PORT=9070
REQUESTS="${REQUESTS:-1000}"
EXTRA_ARGS=("$@")

cleanup() {
    echo "--- Cleaning up ---"
    if [ -n "${SERVICE_PID:-}" ]; then
        kill "$SERVICE_PID" 2>/dev/null || true
        wait "$SERVICE_PID" 2>/dev/null || true
    fi
    docker rm -f "$RESTATE_CONTAINER" 2>/dev/null || true
}
trap cleanup EXIT

echo "=== Building .NET service ==="
dotnet build "$PROJECT_DIR" -c Release --nologo -v quiet

echo "=== Starting Restate server ==="
docker rm -f "$RESTATE_CONTAINER" 2>/dev/null || true
docker run -d \
    --name "$RESTATE_CONTAINER" \
    -p "$RESTATE_INGRESS_PORT:8080" \
    -p "$RESTATE_ADMIN_PORT:9070" \
    -e RESTATE_LOG_FILTER=warn \
    "$RESTATE_IMAGE"

echo "Waiting for Restate admin to be ready..."
for i in $(seq 1 30); do
    if curl -sf "http://localhost:$RESTATE_ADMIN_PORT/health" > /dev/null 2>&1; then
        echo "Restate is ready."
        break
    fi
    if [ "$i" -eq 30 ]; then
        echo "ERROR: Restate failed to start within 30 seconds."
        exit 1
    fi
    sleep 1
done

echo "=== Starting .NET service on port $SERVICE_PORT ==="
dotnet run --project "$PROJECT_DIR" -c Release --no-build -- &
SERVICE_PID=$!

echo "Waiting for .NET service to be ready..."
for i in $(seq 1 15); do
    if curl -sf "http://localhost:$SERVICE_PORT/restate/health" > /dev/null 2>&1; then
        echo "Service is ready."
        break
    fi
    if [ "$i" -eq 15 ]; then
        echo "ERROR: .NET service failed to start within 15 seconds."
        exit 1
    fi
    sleep 1
done

echo "=== Registering service with Restate ==="
curl -sf -X POST "http://localhost:$RESTATE_ADMIN_PORT/deployments" \
    -H 'Content-Type: application/json' \
    -d "{\"uri\": \"http://host.docker.internal:$SERVICE_PORT\"}" \
    > /dev/null

sleep 2

echo "=== Running benchmark ==="
dotnet run --project "$PROJECT_DIR" -c Release --no-build -- \
    --benchmark \
    --url "http://localhost:$RESTATE_INGRESS_PORT" \
    --requests "$REQUESTS" \
    "${EXTRA_ARGS[@]}"

echo "=== Done ==="
