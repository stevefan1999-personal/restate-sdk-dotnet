namespace TicketReservation;

public enum TicketState
{
    Available,
    Reserved,
    Sold
}

public record TicketStatus(TicketState State, string? ReservedBy = null);

public record ReservationRequest(string UserId);

public record CheckoutRequest(string TicketId, string UserId, decimal Price);

public record CheckoutResponse(string OrderId, bool Success);