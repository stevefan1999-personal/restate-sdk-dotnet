using Restate.Sdk.Testing;

namespace Restate.Sdk.Tests.Handlers;

file enum TicketState
{
    Available,
    Reserved,
    Sold
}

// Inline handler for testing — state machine virtual object
[VirtualObject]
file class TicketObject
{
    private static readonly StateKey<TicketState> Status = new("status");
    private static readonly StateKey<string> ReservedBy = new("reservedBy");

    [Handler]
    public async Task<TicketState> Reserve(ObjectContext ctx, string userId)
    {
        var state = await ctx.Get(Status);

        if (state is TicketState.Reserved)
            throw new TerminalException("Ticket is already reserved", 409);
        if (state is TicketState.Sold)
            throw new TerminalException("Ticket is already sold", 409);

        ctx.Set(Status, TicketState.Reserved);
        ctx.Set(ReservedBy, userId);

        // Schedule automatic expiry via delayed self-send
        await ctx.Send("TicketObject", ctx.Key, "Cancel", delay: TimeSpan.FromMinutes(15));

        return TicketState.Reserved;
    }

    [Handler]
    public async Task<TicketState> Confirm(ObjectContext ctx)
    {
        var state = await ctx.Get(Status);

        if (state is not TicketState.Reserved)
            throw new TerminalException("Ticket must be reserved before confirming", 400);

        ctx.Set(Status, TicketState.Sold);
        return TicketState.Sold;
    }

    [Handler]
    public async Task Cancel(ObjectContext ctx)
    {
        var state = await ctx.Get(Status);

        if (state is TicketState.Reserved)
        {
            ctx.Set(Status, TicketState.Available);
            ctx.Clear("reservedBy");
        }
    }

    [SharedHandler]
    public async Task<TicketState> GetStatus(SharedObjectContext ctx)
    {
        return await ctx.Get(Status);
    }
}

public class TicketReservationHandlerTests
{
    [Fact]
    public async Task Reserve_WhenAvailable_SetsReservedState()
    {
        var ctx = new MockObjectContext("ticket-1");

        var ticket = new TicketObject();
        var result = await ticket.Reserve(ctx, "alice");

        Assert.Equal(TicketState.Reserved, result);
        Assert.Equal(TicketState.Reserved, ctx.GetStateValue<TicketState>("status"));
        Assert.Equal("alice", ctx.GetStateValue<string>("reservedBy"));
    }

    [Fact]
    public async Task Reserve_WhenAlreadyReserved_ThrowsTerminalException()
    {
        var ctx = new MockObjectContext("ticket-1");
        ctx.SetupState(new StateKey<TicketState>("status"), TicketState.Reserved);

        var ticket = new TicketObject();

        var ex = await Assert.ThrowsAsync<TerminalException>(() => ticket.Reserve(ctx, "bob"));

        Assert.Equal(409, ex.Code);
        Assert.Contains("already reserved", ex.Message);
    }

    [Fact]
    public async Task Reserve_WhenSold_ThrowsTerminalException()
    {
        var ctx = new MockObjectContext("ticket-1");
        ctx.SetupState(new StateKey<TicketState>("status"), TicketState.Sold);

        var ticket = new TicketObject();

        var ex = await Assert.ThrowsAsync<TerminalException>(() => ticket.Reserve(ctx, "bob"));

        Assert.Equal(409, ex.Code);
    }

    [Fact]
    public async Task Reserve_SendsDelayedCancelForExpiry()
    {
        var ctx = new MockObjectContext("ticket-1");

        var ticket = new TicketObject();
        await ticket.Reserve(ctx, "alice");

        Assert.Single(ctx.Sends);
        var send = ctx.Sends[0];
        Assert.Equal("TicketObject", send.Service);
        Assert.Equal("ticket-1", send.Key);
        Assert.Equal("Cancel", send.Handler);
        Assert.Equal(TimeSpan.FromMinutes(15), send.Delay);
    }

    [Fact]
    public async Task Confirm_WhenReserved_TransitionsToSold()
    {
        var ctx = new MockObjectContext("ticket-1");
        ctx.SetupState(new StateKey<TicketState>("status"), TicketState.Reserved);
        ctx.SetupState(new StateKey<string>("reservedBy"), "alice");

        var ticket = new TicketObject();
        var result = await ticket.Confirm(ctx);

        Assert.Equal(TicketState.Sold, result);
        Assert.Equal(TicketState.Sold, ctx.GetStateValue<TicketState>("status"));
    }

    [Fact]
    public async Task Confirm_WhenNotReserved_ThrowsTerminalException()
    {
        var ctx = new MockObjectContext("ticket-1");
        // Default state: Available

        var ticket = new TicketObject();

        var ex = await Assert.ThrowsAsync<TerminalException>(() => ticket.Confirm(ctx));

        Assert.Equal(400, ex.Code);
    }

    [Fact]
    public async Task Cancel_WhenReserved_TransitionsToAvailable()
    {
        var ctx = new MockObjectContext("ticket-1");
        ctx.SetupState(new StateKey<TicketState>("status"), TicketState.Reserved);
        ctx.SetupState(new StateKey<string>("reservedBy"), "alice");

        var ticket = new TicketObject();
        await ticket.Cancel(ctx);

        Assert.Equal(TicketState.Available, ctx.GetStateValue<TicketState>("status"));
        Assert.False(ctx.HasState("reservedBy"));
    }

    [Fact]
    public async Task Cancel_WhenAvailable_IsNoOp()
    {
        var ctx = new MockObjectContext("ticket-1");
        // Default state: Available (no state set)

        var ticket = new TicketObject();
        await ticket.Cancel(ctx);

        // State remains unchanged — no error thrown
        Assert.False(ctx.HasState("reservedBy"));
    }

    [Fact]
    public async Task Cancel_WhenSold_IsNoOp()
    {
        var ctx = new MockObjectContext("ticket-1");
        ctx.SetupState(new StateKey<TicketState>("status"), TicketState.Sold);

        var ticket = new TicketObject();
        await ticket.Cancel(ctx);

        // Sold tickets can't be cancelled — state stays Sold
        Assert.Equal(TicketState.Sold, ctx.GetStateValue<TicketState>("status"));
    }

    [Fact]
    public async Task GetStatus_ReturnsCurrentState()
    {
        var ctx = new MockSharedObjectContext("ticket-1");
        ctx.SetupState(new StateKey<TicketState>("status"), TicketState.Reserved);

        var ticket = new TicketObject();
        var result = await ticket.GetStatus(ctx);

        Assert.Equal(TicketState.Reserved, result);
    }

    [Fact]
    public async Task GetStatus_DefaultsToAvailable()
    {
        var ctx = new MockSharedObjectContext("ticket-new");

        var ticket = new TicketObject();
        var result = await ticket.GetStatus(ctx);

        Assert.Equal(TicketState.Available, result);
    }
}