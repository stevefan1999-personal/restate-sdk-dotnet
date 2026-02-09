using Restate.Sdk;

namespace Greeter;

/// <summary>
///     A stateless Restate Service demonstrating durable execution fundamentals:
///     side effects with ctx.Run() and durable timers with ctx.Sleep().
///     Services are stateless and can handle unlimited concurrent invocations.
///     Every operation inside a handler is journaled — if the process crashes,
///     Restate replays the journal so completed steps are skipped automatically.
/// </summary>
[Service]
public sealed class GreeterService
{
    /// <summary>
    ///     Greets a user with a durable side effect and a timer.
    ///     ctx.Run() wraps non-deterministic operations (I/O, timestamps, random values)
    ///     so their result is journaled. On replay, the journaled result is returned
    ///     without re-executing the operation — preventing duplicate API calls or emails.
    ///     ctx.Sleep() is a durable timer — the process can shut down during the sleep
    ///     and Restate will wake it up at the right time.
    /// </summary>
    [Handler]
    public async Task<GreetResponse> Greet(Context ctx, GreetRequest request)
    {
        // Side effect: result is journaled. On retry/replay, this won't re-execute.
        var greeting = await ctx.Run("generate-greeting",
            () => GreetingGenerator.Generate(request.Name));

        // Durable timer: survives process restarts. Restate resumes after the delay.
        await ctx.Sleep(TimeSpan.FromSeconds(1));

        return new GreetResponse(greeting);
    }

    /// <summary>
    ///     Demonstrates the IRunContext overload which provides a CancellationToken
    ///     inside the side effect — useful for calling external APIs that support cancellation.
    /// </summary>
    [Handler]
    public async Task<GreetResponse> GreetWithCancellation(Context ctx, GreetRequest request)
    {
        var greeting = await ctx.Run("generate-greeting", runCtx =>
        {
            // The CancellationToken is triggered if the invocation is cancelled
            runCtx.CancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(GreetingGenerator.GenerateWithTimestamp(request.Name));
        });

        return new GreetResponse(greeting);
    }
}