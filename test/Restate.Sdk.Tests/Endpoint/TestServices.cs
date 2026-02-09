namespace Restate.Sdk.Tests.Endpoint;

[Service]
public class GreeterService
{
    [Handler]
    public Task<string> Greet(Context ctx, string name)
    {
        return Task.FromResult($"Hello {name}!");
    }

    [Handler]
    public Task SayHello(Context ctx)
    {
        return Task.CompletedTask;
    }
}

[Service(Name = "CustomName")]
public class NamedService
{
    [Handler(Name = "CustomHandler")]
    public Task<int> DoWork(Context ctx, int input)
    {
        return Task.FromResult(input * 2);
    }
}

[VirtualObject]
public class CounterObject
{
    [Handler]
    public Task<int> Add(ObjectContext ctx, int delta)
    {
        return Task.FromResult(delta);
    }

    [SharedHandler]
    public Task<int> Get(SharedObjectContext ctx)
    {
        return Task.FromResult(0);
    }
}

[Workflow]
public class OrderWorkflow
{
    [Handler]
    public Task<string> Run(WorkflowContext ctx)
    {
        return Task.FromResult("completed");
    }

    [SharedHandler]
    public Task<string> GetStatus(SharedWorkflowContext ctx)
    {
        return Task.FromResult("pending");
    }
}

public class NotAService
{
    public void DoSomething()
    {
    }
}

[Service]
public class EmptyService
{
}

[Service]
public class VoidHandlerService
{
    [Handler]
    public void DoNothing(Context ctx)
    {
    }
}

[Service]
public class ValueTaskService
{
    [Handler]
    public ValueTask<int> GetNumber(Context ctx)
    {
        return new ValueTask<int>(42);
    }

    [Handler]
    public ValueTask DoWork(Context ctx)
    {
        return ValueTask.CompletedTask;
    }
}