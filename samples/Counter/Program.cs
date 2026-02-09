using Counter;
using Restate.Sdk.Hosting;

// Quick-start: self-hosted Restate endpoint with a single Virtual Object.
// Register at http://localhost:9081 then interact via the Restate CLI or HTTP:
//   restate invocations invoke CounterObject/my-counter Add --body '5'
//   restate invocations invoke CounterObject/my-counter Get
await RestateHost.CreateBuilder()
    .AddVirtualObject<CounterObject>()
    .WithPort(9081)
    .Build()
    .RunAsync();