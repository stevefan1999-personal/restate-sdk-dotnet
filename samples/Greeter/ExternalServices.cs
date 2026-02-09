namespace Greeter;

/// <summary>
///     Stub for an external greeting API. In production this might call a
///     third-party service, database, or ML model to generate personalized greetings.
/// </summary>
public static class GreetingGenerator
{
    public static string Generate(string name)
    {
        return $"Hello, {name}! Welcome aboard.";
    }

    public static string GenerateWithTimestamp(string name)
    {
        return $"Hello, {name}! The time is {DateTime.UtcNow:HH:mm:ss} UTC.";
    }
}