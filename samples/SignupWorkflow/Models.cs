namespace SignupWorkflow;

public record SignupRequest(string Email, string Name);

public record SignupResult(string AccountId, bool Verified);