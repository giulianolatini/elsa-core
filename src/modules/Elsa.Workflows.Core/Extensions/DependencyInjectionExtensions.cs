using Elsa.Features.Services;
using Elsa.Workflows.Core.Features;
using Elsa.Workflows.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Elsa.Workflows.Core;

public static class DependencyInjectionExtensions
{
    public static IModule UseWorkflows(this IModule configuration, Action<WorkflowsFeature>? configure = default)
    {
        configuration.Configure(configure);
        return configuration;
    }

    public static IServiceCollection AddDataDrive<T>(this IServiceCollection services) where T : class, IDataDrive
    {
        return services.AddSingleton<IDataDrive, T>();
    }
}