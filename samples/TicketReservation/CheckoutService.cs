using Restate.Sdk;

namespace TicketReservation;

/// <summary>
///     A stateless Service that orchestrates the checkout flow:
///     process payment, then confirm the ticket reservation.
///     Demonstrates:
///     - ctx.Run() for idempotent side effects (payment)
///     - ctx.Call() for cross-service RPC to the TicketObject
///     - Error handling with compensation (refund on failure)
/// </summary>
[Service]
public sealed class CheckoutService
{
    /// <summary>
    ///     Processes payment and confirms the ticket reservation.
    ///     If confirmation fails, the payment is refunded automatically.
    /// </summary>
    [Handler]
    public async Task<CheckoutResponse> Checkout(Context ctx, CheckoutRequest request)
    {
        // Step 1: Process payment via a durable side effect.
        // If this handler retries, the journaled result is returned (no double charge).
        var paymentId = await ctx.Run("process-payment",
            () => PaymentGateway.Charge(request.UserId, request.Price));

        try
        {
            // Step 2: Confirm the ticket reservation via cross-service call.
            // Restate guarantees exactly-once delivery.
            await ctx.Call<TicketStatus>("TicketObject", request.TicketId, "Confirm");
        }
        catch (TerminalException)
        {
            // Compensation: refund the payment if the ticket can't be confirmed.
            await ctx.Run("refund-payment", () =>
            {
                PaymentGateway.Refund(paymentId);
                return Task.CompletedTask;
            });

            return new CheckoutResponse($"order-failed-{request.TicketId}", false);
        }

        return new CheckoutResponse($"order-{request.TicketId}", true);
    }
}