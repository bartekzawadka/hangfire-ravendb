namespace Hangfire.Raven.Samples.Web;

public class SchedulingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        BackgroundJob.Enqueue(() => Console.WriteLine("Background Job: Hello, world!"));

        BackgroundJob.Enqueue(() => TestJobs.QueueTest());

        BackgroundJob.Schedule(() => Console.WriteLine("Scheduled Job: Hello, I am delayed world!"), new TimeSpan(0, 1, 0));

        BackgroundJob.Enqueue(() => TestJobs.FailingTest());

        // Run every minute
        RecurringJob.AddOrUpdate("minutely", () => TestJobs.CronTest(), Cron.Minutely);

        await Task.Delay(1000, stoppingToken)
            .ContinueWith(
                _ =>
                {
                    for (var i = 0; i < 50; i++)
                        BackgroundJob.Enqueue(() => Console.WriteLine("Background Job: Hello stressed world!"));

                    for (var i = 0; i < 100; i++)
                        BackgroundJob.Enqueue(() => TestJobs.WorkerCountTest());
                },
                stoppingToken);
    }
}
