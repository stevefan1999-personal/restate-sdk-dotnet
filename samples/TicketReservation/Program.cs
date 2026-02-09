using Restate.Sdk.Hosting;
using TicketReservation;

// Concert ticket reservation system with automatic expiry.
// Register at http://localhost:9082 then:
//   restate invocations invoke TicketObject/ticket-1 Reserve --body '{"userId": "alice"}'
//   restate invocations invoke CheckoutService Checkout --body '{"ticketId": "ticket-1", "userId": "alice", "price": 99.99}'
//   restate invocations invoke TicketObject/ticket-1 GetStatus
await RestateHost.CreateBuilder()
    .AddVirtualObject<TicketObject>()
    .AddService<CheckoutService>()
    .WithPort(9082)
    .Build()
    .RunAsync();