# Restate SDK Cross-Language Benchmark Specification v1.0

## Purpose
Standard scenarios for comparing Restate SDK performance across languages (Java, Go, Rust, TypeScript, Python, .NET). Each scenario isolates SDK processing overhead from network I/O.

## Scenarios

### 1. Noop
Handler receives input, returns it immediately. No durable operations. Measures pure SDK dispatch overhead.

### 2. SingleRun
Handler calls ctx.run() once with a string result. Measures: command serialization + journal append + flush.

### 3. ThreeRuns
Handler calls ctx.run() three times (string, int, DTO). Measures sustained operation throughput.

### 4. StateGetSet
Handler on a virtual object: get state, set state, return value. Measures state operation overhead.

### 5. Call
Handler calls another service and awaits response. Measures: call command build + completion await.

### 6. Sleep
Handler sleeps for 1 second. Measures: timer command + completion await (completion provided immediately in mock).

### 7. ReplayRun_5Entries
Handler replays 5 previously journaled run entries. Measures journal replay performance.

## Measurement Methodology
- Use in-memory protocol (mock reader/writer) to isolate SDK processing from I/O
- Language-specific tools: BenchmarkDotNet (.NET), JMH (Java), testing.B (Go), Criterion (Rust), benchmark.js (TS)
- Minimum 3 warmup iterations + 10 measurement iterations
- Report: ops/sec, p50/p95/p99 latency (ns), allocation bytes

## Reporting Format (JSON)
```json
{"scenario":"SingleRun","language":"dotnet","ops_per_sec":500000,"p50_ns":1800,"p95_ns":2500,"p99_ns":4000,"alloc_bytes":128}
```

## Infrastructure
- Pin Restate server version across all SDKs
- Same machine/container specs for all runs
- Report CPU model, RAM, OS in results

## Avoiding False Positives
- JIT warmup handled by benchmark frameworks (BenchmarkDotNet, JMH auto-warmup)
- GC: use MemoryDiagnoser/.NET, -gc profiler/Java, pprof/Go
- Report percentiles not averages (hides GC pauses)
- For end-to-end: use Counter.GetAndAdd (matches Restate runtime benchmarks)
