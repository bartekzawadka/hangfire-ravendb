namespace Hangfire.Raven.Samples.Web;

public class TestJobs
{
    private readonly ILogger<TestJobs> _logger;

    public TestJobs(ILogger<TestJobs> logger)
    {
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public void CronTest() => _logger.LogInformation("Cron Job: Hello, world!");

    [Queue("testing")]
    public void QueueTest() => _logger.LogInformation("Queue test Job: Hello, world!");

    [Queue("testing")]
    public void FailingTest() {
        _logger.LogInformation("Requeue test!");
        throw new Exception("FAILING TEST EXCEPTION!");
    }

    [Queue("testing")]
    public Task DelayedTest(CancellationToken cancellationToken)
        => Task.Delay(1000, cancellationToken).ContinueWith(_ => _logger.LogInformation("Delayed job: Hello, world!"), cancellationToken);
}
