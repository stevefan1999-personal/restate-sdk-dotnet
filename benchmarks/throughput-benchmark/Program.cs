using System.Diagnostics;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Restate.Sdk.Hosting;
using Restate.Sdk.ThroughputBenchmark;

if (args.Contains("--benchmark"))
{
    await RunBenchmark(args);
}
else
{
    await RestateHost.CreateBuilder()
        .Bind<CounterObject>()
        .WithPort(9090)
        .Build()
        .RunAsync();
}

static async Task RunBenchmark(string[] args)
{
    var baseUrl = "http://localhost:8080";
    var totalRequests = 1000;
    var warmupRequests = 100;
    var parallel = 1;
    var sequential = true;

    for (var i = 0; i < args.Length; i++)
    {
        switch (args[i])
        {
            case "--url" when i + 1 < args.Length:
                baseUrl = args[++i];
                break;
            case "--requests" when i + 1 < args.Length:
                totalRequests = int.Parse(args[++i], CultureInfo.InvariantCulture);
                break;
            case "--warmup" when i + 1 < args.Length:
                warmupRequests = int.Parse(args[++i], CultureInfo.InvariantCulture);
                break;
            case "--parallel" when i + 1 < args.Length:
                parallel = int.Parse(args[++i], CultureInfo.InvariantCulture);
                sequential = false;
                break;
            case "--sequential":
                sequential = true;
                parallel = 1;
                break;
        }
    }

    using var client = new HttpClient { BaseAddress = new Uri(baseUrl) };

    Console.Error.WriteLine($"Warming up with {warmupRequests} requests...");
    await RunRequests(client, warmupRequests, sequential ? 1 : parallel, "warmup");

    Console.Error.WriteLine($"Running {totalRequests} requests (parallel={parallel}, sequential={sequential})...");
    var (latencies, elapsed) = await RunRequests(client, totalRequests, parallel, "bench");

    Array.Sort(latencies);

    var opsPerSec = totalRequests / elapsed.TotalSeconds;
    var p50 = Percentile(latencies, 0.50);
    var p95 = Percentile(latencies, 0.95);
    var p99 = Percentile(latencies, 0.99);
    var min = latencies[0];
    var max = latencies[^1];
    var avg = latencies.Average();

    var result = new
    {
        scenario = "CounterGetAndAdd",
        language = "dotnet",
        mode = sequential ? "sequential" : $"parallel-{parallel}",
        total_requests = totalRequests,
        ops_per_sec = Math.Round(opsPerSec, 1),
        p50_ms = Math.Round(p50, 3),
        p95_ms = Math.Round(p95, 3),
        p99_ms = Math.Round(p99, 3),
        min_ms = Math.Round(min, 3),
        max_ms = Math.Round(max, 3),
        avg_ms = Math.Round(avg, 3),
        elapsed_sec = Math.Round(elapsed.TotalSeconds, 3)
    };

    Console.WriteLine(JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }));
}

static async Task<(double[] Latencies, TimeSpan Elapsed)> RunRequests(
    HttpClient client, int count, int parallelism, string keyPrefix)
{
    var latencies = new double[count];
    var sw = Stopwatch.StartNew();

    if (parallelism <= 1)
    {
        for (var i = 0; i < count; i++)
        {
            var reqSw = Stopwatch.StartNew();
            var key = $"{keyPrefix}-0";
            var response = await client.PostAsJsonAsync($"/CounterObject/{key}/GetAndAdd", 1);
            response.EnsureSuccessStatusCode();
            reqSw.Stop();
            latencies[i] = reqSw.Elapsed.TotalMilliseconds;
        }
    }
    else
    {
        var semaphore = new SemaphoreSlim(parallelism);
        var tasks = new Task[count];

        for (var i = 0; i < count; i++)
        {
            var index = i;
            tasks[i] = Task.Run(async () =>
            {
                await semaphore.WaitAsync();
                try
                {
                    var reqSw = Stopwatch.StartNew();
                    var key = $"{keyPrefix}-{index % parallelism}";
                    var response = await client.PostAsJsonAsync($"/CounterObject/{key}/GetAndAdd", 1);
                    response.EnsureSuccessStatusCode();
                    reqSw.Stop();
                    latencies[index] = reqSw.Elapsed.TotalMilliseconds;
                }
                finally
                {
                    semaphore.Release();
                }
            });
        }

        await Task.WhenAll(tasks);
    }

    sw.Stop();
    return (latencies, sw.Elapsed);
}

static double Percentile(double[] sorted, double p)
{
    var index = (int)Math.Ceiling(p * sorted.Length) - 1;
    return sorted[Math.Max(0, index)];
}
