using Hangfire.Raven.Storage;

namespace Hangfire.Raven.Samples.Console
{
    public static class Program
    {
        private static int _x;

        public static void Main()
        {
            // you can use Raven Storage and specify the connection string name
            //GlobalConfiguration.Configuration
            //    .UseColouredConsoleLogProvider()
            //    .UseRavenStorage("RavenDebug");

            // you can use Raven Storage and specify the connection string and database name
            GlobalConfiguration.Configuration
                .UseColouredConsoleLogProvider()
                .UseRavenStorage("http://localhost:8080", "HangfireConsole");

            // you can use Raven Embedded Storage which runs in memory!
            //GlobalConfiguration.Configuration
            //    .UseColouredConsoleLogProvider()
            //    .UseEmbeddedRavenStorage();

            //you have to create an instance of background job server at least once for background jobs to run
            _ = new BackgroundJobServer();

            // Run once
            BackgroundJob.Enqueue(() => System.Console.WriteLine("Background Job: Hello, world!"));

            BackgroundJob.Enqueue(() => Test());

            // Run every minute
            RecurringJob.AddOrUpdate("minutely", () => Test(), Cron.Minutely);

            System.Console.WriteLine("Press Enter to exit...");
            System.Console.ReadLine();
        }

        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
        public static void Test()
        {
            System.Console.WriteLine($"{_x++} Cron Job: Hello, world!");
            //throw new ArgumentException("fail");
        }
    }
}