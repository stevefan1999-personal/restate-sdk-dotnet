using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Restate.Sdk.Internal;

namespace Restate.Sdk.Hosting;

/// <summary>
///     Extension methods for registering Restate services with dependency injection.
/// </summary>
public static class RestateServiceCollectionExtensions
{
    /// <summary>
    ///     Adds Restate services to the dependency injection container.
    /// </summary>
    /// <example>
    ///     <code>
    /// builder.Services.AddRestate(opts =&gt; opts.AddService&lt;GreeterService&gt;());
    /// </code>
    /// </example>
    [RequiresUnreferencedCode("AddRestate uses reflection-based DI registration.")]
    public static IServiceCollection AddRestate(
        this IServiceCollection services,
        Action<RestateOptions> configure)
    {
        var options = new RestateOptions();
        configure(options);

        var registry = ServiceRegistry.FromTypes(options.ServiceTypes);
        services.AddSingleton(registry);
        services.TryAddSingleton<InvocationHandler>();

        foreach (var type in options.ServiceTypes)
            services.TryAddScoped(type);

        return services;
    }
}