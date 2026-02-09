using Greeter;
using Restate.Sdk.Hosting;

// Quick-start: self-hosted Restate endpoint with a stateless service.
// Register at http://localhost:9080 then call:
//   restate invocations invoke GreeterService Greet --body '{"name": "Alice"}'
await RestateHost.CreateBuilder()
    .AddService<GreeterService>()
    .Build()
    .RunAsync();