using Restate.Sdk;

namespace TicketReservation;

/// <summary>
///     A Virtual Object modeling a concert ticket with a state machine lifecycle:
///     Available → Reserved → Sold
///     ↓
///     Available (on cancel/expiry)
///     This is the canonical Restate example — present in every SDK (TypeScript, Java,
///     Python, Go, Rust). It demonstrates:
///     - State machine transitions with durable state
///     - Delayed self-sends for reservation expiry
///     - TerminalException for non-retryable business errors
///     - Shared handlers for concurrent read access
/// </summary>
[VirtualObject]
public sealed class TicketObject
{
    private static readonly StateKey<TicketState> Status = new("status");
    private static readonly StateKey<string> ReservedBy = new("reservedBy");

    /// <summary>How long a reservation remains valid before automatic expiry.</summary>
    private static readonly TimeSpan ReservationExpiry = TimeSpan.FromMinutes(15);

    /// <summary>
    ///     Reserves the ticket for a user. If the ticket is already reserved or sold,
    ///     a TerminalException is thrown (non-retryable — Restate won't retry this).
    ///     Schedules automatic cancellation after 15 minutes by sending a delayed
    ///     message to the Cancel handler. This is a durable timer — even if this
    ///     process crashes, Restate will deliver the message at the right time.
    /// </summary>
    [Handler]
    public async Task<TicketStatus> Reserve(ObjectContext ctx, ReservationRequest request)
    {
        var state = await ctx.Get(Status);

        if (state is TicketState.Reserved)
            throw new TerminalException("Ticket is already reserved", 409);
        if (state is TicketState.Sold)
            throw new TerminalException("Ticket is already sold", 409);

        ctx.Set(Status, TicketState.Reserved);
        ctx.Set(ReservedBy, request.UserId);

        // Schedule automatic expiry: sends a Cancel message to this same object key
        // after 15 minutes. Durable — survives process restarts.
        await ctx.Send("TicketObject", ctx.Key, "Cancel", delay: ReservationExpiry);

        return new TicketStatus(TicketState.Reserved, request.UserId);
    }

    /// <summary>
    ///     Confirms the reservation, transitioning the ticket to Sold.
    ///     Only valid when the ticket is currently Reserved.
    /// </summary>
    [Handler]
    public async Task<TicketStatus> Confirm(ObjectContext ctx)
    {
        var state = await ctx.Get(Status);

        if (state is not TicketState.Reserved)
            throw new TerminalException("Ticket must be reserved before confirming", 400);

        ctx.Set(Status, TicketState.Sold);

        return new TicketStatus(TicketState.Sold, await ctx.Get(ReservedBy));
    }

    /// <summary>
    ///     Cancels a reservation, returning the ticket to Available.
    ///     No-op if the ticket is already Available or Sold (idempotent).
    ///     Called either explicitly or by the delayed expiry message.
    /// </summary>
    [Handler]
    public async Task Cancel(ObjectContext ctx)
    {
        var state = await ctx.Get(Status);

        if (state is TicketState.Reserved)
        {
            ctx.Set(Status, TicketState.Available);
            ctx.Clear("reservedBy");
        }
        // If Available or Sold, this is a no-op (idempotent)
    }

    /// <summary>
    ///     Returns the current ticket status. Shared handler — runs concurrently
    ///     with other reads, does not block exclusive handlers.
    /// </summary>
    [SharedHandler]
    public async Task<TicketStatus> GetStatus(SharedObjectContext ctx)
    {
        var state = await ctx.Get(Status);
        var reservedBy = await ctx.Get(ReservedBy);
        return new TicketStatus(state, reservedBy);
    }
}