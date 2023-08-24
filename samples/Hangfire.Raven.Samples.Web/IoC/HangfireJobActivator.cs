namespace Hangfire.Raven.Samples.Web.IoC;

public class HangfireJobActivator : JobActivator
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public HangfireJobActivator(IServiceProvider serviceProvider)
    {
        _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
    }

    public override JobActivatorScope BeginScope(JobActivatorContext context) => new HangfireJobActivatorScope(_serviceScopeFactory.CreateScope());
}
