using Microsoft.CodeAnalysis;

namespace Restate.Sdk.Generators.Tests;

public class ClientGeneratorTests
{
    [Fact]
    public void Service_GeneratesClientInterface()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class GreeterService
                     {
                         [Handler]
                         public Task<string> Greet(Context ctx, string name) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var generated = GeneratorTestHelper.GetGeneratedSource(driver, "GreeterServiceClient.g.cs");

        Assert.NotNull(generated);
        Assert.Contains("public interface IGreeterServiceClient", generated);
        Assert.Contains("public interface IGreeterServiceSendClient", generated);
        Assert.Contains("GreetAsync(", generated);
        Assert.Contains("GreetSend(", generated);
    }

    [Fact]
    public void VirtualObject_KeyBoundClient()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [VirtualObject]
                     public class CounterObject
                     {
                         [Handler]
                         public Task<int> Add(ObjectContext ctx, int delta) => Task.FromResult(delta);

                         [SharedHandler]
                         public Task<int> Get(SharedObjectContext ctx) => Task.FromResult(0);
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var generated = GeneratorTestHelper.GetGeneratedSource(driver, "CounterObjectClient.g.cs");

        Assert.NotNull(generated);
        // Key is stored in constructor, not per-method
        Assert.Contains("private readonly string _key;", generated);
        Assert.Contains("ICounterObjectClient", generated);
        Assert.Contains("ICounterObjectSendClient", generated);
        // Methods should NOT have key parameter
        Assert.Contains("AddAsync(int request)", generated);
        Assert.Contains("GetAsync()", generated);
    }

    [Fact]
    public void Workflow_GeneratesKeyBoundClient()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Workflow]
                     public class OrderWorkflow
                     {
                         [Handler]
                         public Task<string> Run(WorkflowContext ctx) => Task.FromResult("done");

                         [SharedHandler]
                         public Task<string> GetStatus(SharedWorkflowContext ctx) => Task.FromResult("pending");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var generated = GeneratorTestHelper.GetGeneratedSource(driver, "OrderWorkflowClient.g.cs");

        Assert.NotNull(generated);
        Assert.Contains("private readonly string _key;", generated);
        Assert.Contains("IOrderWorkflowClient", generated);
        Assert.Contains("IOrderWorkflowSendClient", generated);
    }

    [Fact]
    public void CustomName_UsesOverride()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service(Name = "MyGreeter")]
                     public class GreeterService
                     {
                         [Handler(Name = "SayHi")]
                         public Task<string> Greet(Context ctx) => Task.FromResult("Hi");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var generated = GeneratorTestHelper.GetGeneratedSource(driver, "GreeterServiceClient.g.cs");

        Assert.NotNull(generated);
        Assert.Contains("\"MyGreeter\"", generated);
        Assert.Contains("\"SayHi\"", generated);
        Assert.Contains("SayHiAsync", generated);
    }

    [Fact]
    public void VoidHandler_GeneratesValueTaskReturn()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class MyService
                     {
                         [Handler]
                         public Task DoWork(Context ctx) => Task.CompletedTask;
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var generated = GeneratorTestHelper.GetGeneratedSource(driver, "MyServiceClient.g.cs");

        Assert.NotNull(generated);
        Assert.Contains("ValueTask DoWorkAsync()", generated);
    }

    [Fact]
    public void NoServiceAttribute_GeneratesNothing()
    {
        var source = """
                     namespace TestApp;

                     public class RegularClass
                     {
                         public void DoSomething() { }
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var result = driver.GetRunResult();

        Assert.Empty(result.GeneratedTrees);
    }

    [Fact]
    public void NoDiagnosticErrors()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class GreeterService
                     {
                         [Handler]
                         public Task<string> Greet(Context ctx, string name) => Task.FromResult("Hello");
                     }
                     """;

        var (_, _, diagnostics) = GeneratorTestHelper.RunGenerator(source);

        var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
        Assert.Empty(errors);
    }

    [Fact]
    public void Implementation_DelegatesToContextCall()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class GreeterService
                     {
                         [Handler]
                         public Task<string> Greet(Context ctx, string name) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var generated = GeneratorTestHelper.GetGeneratedSource(driver, "GreeterServiceClient.g.cs");

        Assert.NotNull(generated);
        Assert.Contains("_context.Call<", generated);
        Assert.Contains("\"GreeterService\"", generated);
        Assert.Contains("\"Greet\"", generated);
    }

    [Fact]
    public void SendClient_DelegatesToContextSend()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class GreeterService
                     {
                         [Handler]
                         public Task<string> Greet(Context ctx, string name) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var generated = GeneratorTestHelper.GetGeneratedSource(driver, "GreeterServiceClient.g.cs");

        Assert.NotNull(generated);
        Assert.Contains("_context.Send(", generated);
        Assert.Contains("_options", generated);
    }

    [Fact]
    public void Diagnostic_MultipleInputParameters()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class BadService
                     {
                         [Handler]
                         public Task<string> Greet(Context ctx, string name, int count) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var diagnostics = GeneratorTestHelper.GetGeneratorDiagnostics(driver);

        Assert.Contains(diagnostics, d => d.Id == "RESTATE001");
    }

    [Fact]
    public void Diagnostic_MissingContextParameter()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class BadService
                     {
                         [Handler]
                         public Task<string> Greet(string name) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var diagnostics = GeneratorTestHelper.GetGeneratorDiagnostics(driver);

        Assert.Contains(diagnostics, d => d.Id == "RESTATE002");
    }

    [Fact]
    public void NestedServiceClass_EmitsRESTATE003()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     public class Outer
                     {
                         [Service]
                         public class NestedService
                         {
                             [Handler]
                             public Task<string> Greet(Context ctx) => Task.FromResult("Hello");
                         }
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var diagnostics = GeneratorTestHelper.GetGeneratorDiagnostics(driver);

        // Nested classes emit RESTATE003 and no code is generated
        Assert.Contains(diagnostics, d => d.Id == "RESTATE003");
        Assert.Single(diagnostics);
    }

    [Fact]
    public void NonPublicServiceClass_EmitsRESTATE008()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     internal class InternalService
                     {
                         [Handler]
                         public Task<string> Greet(Context ctx) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var diagnostics = GeneratorTestHelper.GetGeneratorDiagnostics(driver);

        // Non-public classes emit RESTATE008 and no code is generated
        Assert.Contains(diagnostics, d => d.Id == "RESTATE008");
        Assert.Single(diagnostics);
    }

    [Fact]
    public void AbstractServiceClass_EmitsRESTATE008()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public abstract class AbstractService
                     {
                         [Handler]
                         public Task<string> Greet(Context ctx) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var diagnostics = GeneratorTestHelper.GetGeneratorDiagnostics(driver);

        // Abstract classes emit RESTATE008 and no code is generated
        Assert.Contains(diagnostics, d => d.Id == "RESTATE008");
        Assert.Single(diagnostics);
    }

    [Fact]
    public void Diagnostic_ValidService_NoDiagnostics()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class GoodService
                     {
                         [Handler]
                         public Task<string> Greet(Context ctx, string name) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var diagnostics = GeneratorTestHelper.GetGeneratorDiagnostics(driver);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Service_GeneratesFutureMethods()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class GreeterService
                     {
                         [Handler]
                         public Task<string> Greet(Context ctx, string name) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var generated = GeneratorTestHelper.GetGeneratedSource(driver, "GreeterServiceClient.g.cs");

        Assert.NotNull(generated);
        // Future method on interface
        Assert.Contains("IDurableFuture<string>", generated);
        Assert.Contains("GreetFuture(", generated);
        // Future method delegates to CallFuture
        Assert.Contains("CallFuture<", generated);
    }

    [Fact]
    public void VoidHandler_NoFutureMethod()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class MyService
                     {
                         [Handler]
                         public Task DoWork(Context ctx) => Task.CompletedTask;
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var generated = GeneratorTestHelper.GetGeneratedSource(driver, "MyServiceClient.g.cs");

        Assert.NotNull(generated);
        // Void handlers should NOT get Future methods
        Assert.DoesNotContain("DoWorkFuture", generated);
    }

    [Fact]
    public void VirtualObject_FutureMethod_UsesKey()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [VirtualObject]
                     public class CounterObject
                     {
                         [Handler]
                         public Task<int> Add(ObjectContext ctx, int delta) => Task.FromResult(delta);
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var generated = GeneratorTestHelper.GetGeneratedSource(driver, "CounterObjectClient.g.cs");

        Assert.NotNull(generated);
        Assert.Contains("AddFuture(", generated);
        // Future call should include _key
        Assert.Contains("_context.CallFuture<int>", generated);
    }

    [Fact]
    public void Diagnostic_InvalidTimeSpanFormat_EmitsRESTATE009()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class TimerService
                     {
                         [Handler(InactivityTimeout = "invalid")]
                         public Task<string> Greet(Context ctx, string name) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var diagnostics = GeneratorTestHelper.GetGeneratorDiagnostics(driver);

        Assert.Contains(diagnostics, d => d.Id == "RESTATE009");
    }

    [Fact]
    public void Diagnostic_ValidTimeSpanFormat_NoDiagnostics()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class TimerService
                     {
                         [Handler(InactivityTimeout = "00:05:00")]
                         public Task<string> Greet(Context ctx, string name) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var diagnostics = GeneratorTestHelper.GetGeneratorDiagnostics(driver);

        Assert.Empty(diagnostics);
    }

    [Fact]
    public void Diagnostic_InvalidAbortTimeout_EmitsRESTATE009()
    {
        var source = """
                     using Restate.Sdk;
                     using System.Threading.Tasks;

                     namespace TestApp;

                     [Service]
                     public class TimerService
                     {
                         [Handler(AbortTimeout = "xyz")]
                         public Task<string> Greet(Context ctx, string name) => Task.FromResult("Hello");
                     }
                     """;

        var (driver, _, _) = GeneratorTestHelper.RunGenerator(source);
        var diagnostics = GeneratorTestHelper.GetGeneratorDiagnostics(driver);

        Assert.Contains(diagnostics, d => d.Id == "RESTATE009");
    }
}