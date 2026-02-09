namespace TicketReservation;

/// <summary>
///     Stub for an external payment gateway. In production this would call
///     Stripe, Adyen, or similar — always using idempotency keys to prevent
///     duplicate charges on retry.
/// </summary>
public static class PaymentGateway
{
    public static string Charge(string userId, decimal amount)
    {
        // In a real app: call Stripe with an idempotency key
        return $"pay-{Guid.NewGuid():N}";
    }

    public static void Refund(string paymentId)
    {
        // In a real app: refund the charge
    }
}