namespace SignupWorkflow;

/// <summary>
///     Stub for an account management API.
/// </summary>
public static class AccountService
{
    public static string Create(string email, string name)
    {
        return $"acct-{Guid.NewGuid():N}";
    }

    public static void Activate(string accountId)
    {
        // Mark the account as fully active in the database.
    }
}

/// <summary>
///     Stub for an email delivery service. The verification email contains a link
///     with the awakeable ID so the external click can resolve the durable promise.
/// </summary>
public static class EmailService
{
    public static void SendVerification(string email, string callbackId)
    {
        // In production: send an email with a link like
        // https://myapp.com/verify?token={callbackId}
        // When clicked, the frontend calls ResolveAwakeable(callbackId, "verified")
    }
}