using Restate.Sdk;

namespace SignupWorkflow;

/// <summary>
///     A Workflow for user signup with email verification.
///     Workflows have a special Run handler that executes exactly once per workflow ID.
///     Additional shared handlers can query or signal the workflow while it's running.
///     This demonstrates:
///     - Workflow lifecycle with state tracking
///     - Awakeables: durable promises that pause execution until an external event
///     (the user clicking a verification link)
///     - Shared handlers for querying workflow status
///     - ctx.Run() for idempotent side effects (account creation, email sending)
/// </summary>
[Workflow]
public sealed class SignupWorkflow
{
    private static readonly StateKey<string> Status = new("status");
    private static readonly StateKey<string> AccountId = new("accountId");

    /// <summary>
    ///     Main workflow logic. Executes exactly once per workflow ID (the user's email).
    ///     Creates an account, sends a verification email, waits for the user to click
    ///     the link, then activates the account.
    ///     The awakeable (ctx.Awakeable) is a durable promise — the workflow
    ///     suspends while waiting, using zero compute. When the user clicks the
    ///     verification link, an external system resolves the awakeable and the
    ///     workflow resumes automatically.
    /// </summary>
    [Handler]
    public async Task<SignupResult> Run(WorkflowContext ctx, SignupRequest request)
    {
        // Step 1: Create the account
        ctx.Set(Status, "creating-account");

        var accountId = await ctx.Run("create-account",
            () => AccountService.Create(request.Email, request.Name));

        ctx.Set(AccountId, accountId);

        // Step 2: Send verification email with an awakeable callback
        ctx.Set(Status, "awaiting-verification");

        // Awakeable returns an ID and a Value (durable promise).
        // The ID is sent to the external system; the Value is awaited.
        var awakeable = ctx.Awakeable<string>();

        await ctx.Run("send-verification-email",
            () =>
            {
                EmailService.SendVerification(request.Email, awakeable.Id);
                return Task.CompletedTask;
            });

        // The workflow suspends here — no compute charges on FaaS.
        // When the user clicks the link, the external system calls:
        //   ctx.ResolveAwakeable(awakeableId, "verified")
        // or via HTTP: PUT /restate/awakeables/{id}/resolve
        await awakeable.Value;

        // Step 3: Activate the account
        ctx.Set(Status, "activating");

        await ctx.Run("activate-account",
            () =>
            {
                AccountService.Activate(accountId);
                return Task.CompletedTask;
            });

        ctx.Set(Status, "completed");

        return new SignupResult(accountId, true);
    }

    /// <summary>
    ///     Query the current workflow status. Shared handler — runs concurrently,
    ///     does not block the Run handler.
    /// </summary>
    [SharedHandler]
    public async Task<string> GetStatus(SharedWorkflowContext ctx)
    {
        return await ctx.Get(Status) ?? "unknown";
    }
}