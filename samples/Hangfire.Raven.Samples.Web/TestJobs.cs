using System.Diagnostics;

namespace Hangfire.Raven.Samples.Web;

public static class TestJobs
{
    private static int _x;

    [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public static void CronTest() => Debug.WriteLine($"{_x++} Cron Job: Hello, world!");

    [Queue("testing")]
    public static void QueueTest() => Debug.WriteLine($"{_x++} Queue test Job: Hello, world!");

    public static void WorkerCountTest() => Thread.Sleep(5000);

    [Queue("testing")]
    public static void FailingTest() {
        Debug.WriteLine($"{_x++} Requeue test!");
        throw new Exception();
    }
}
