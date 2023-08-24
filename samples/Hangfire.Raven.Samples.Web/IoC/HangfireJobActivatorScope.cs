using Hangfire.Annotations;

namespace Hangfire.Raven.Samples.Web.IoC;

public class HangfireJobActivatorScope : JobActivatorScope
{
    private readonly IServiceScope _serviceScope;

    public HangfireJobActivatorScope([NotNull] IServiceScope serviceScope)
    {
        _serviceScope = serviceScope;
    }

    public override object Resolve(Type type) => ActivatorUtilities.GetServiceOrCreateInstance(_serviceScope.ServiceProvider, type);
}
