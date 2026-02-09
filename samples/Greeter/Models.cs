namespace Greeter;

public record GreetRequest(string Name);

public record GreetResponse(string Message);