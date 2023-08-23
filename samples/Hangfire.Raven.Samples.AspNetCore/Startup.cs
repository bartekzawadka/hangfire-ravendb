using System.Diagnostics;
using Hangfire.Raven.Storage;
using Microsoft.Extensions.Logging.Console;

namespace Hangfire.Raven.Samples.AspNetCore {
    public class Startup {
        public Startup(IWebHostEnvironment env) {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services) {
            // Add framework services.
            services.AddMvc(options =>
            {
                options.EnableEndpointRouting = false;
            });

            // Add Hangfire to services using RavenDB Storage
            // BUG: https://github.com/cady-io/hangfire-ravendb/issues/15
            //services.AddHangfire(t => t.UseRavenStorage(Configuration["ConnectionStrings:RavenDebug"]));

            services.AddHangfire(t => t.UseRavenStorage(Configuration["ConnectionStrings:RavenDebugUrl"], Configuration["ConnectionStrings:RavenDebugDatabase"]));
            services.AddHangfireServer(options =>
            {
                options.Queues = new[] { "default", "testing" };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory) {
            // loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            // loggerFactory.AddProvider(new ConsoleLoggerProvider());
            // loggerFactory.AddDebug();

            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
                //app.Brow
            } else {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            // Add Hangfire Server and Dashboard support
            //app.UseHangfireServer(new BackgroundJobServerOptions() { Queues = new[] { "default", "testing" } });
            app.UseHangfireDashboard();

            // Run once
            BackgroundJob.Enqueue(() => System.Console.WriteLine("Background Job: Hello, world!"));

            BackgroundJob.Enqueue(() => QueueTest());

            BackgroundJob.Schedule(() => System.Console.WriteLine("Scheduled Job: Hello, I am delayed world!"), new System.TimeSpan(0, 1, 0));

            BackgroundJob.Enqueue(() => FailingTest());

            // Run every minute
            RecurringJob.AddOrUpdate("minutely", () => CronTest(), Cron.Minutely);


            Task.Delay(1000).ContinueWith((task) => {
                for (int i = 0; i < 50; i++)
                    BackgroundJob.Enqueue(() => System.Console.WriteLine("Background Job: Hello stressed world!"));

                for (int i = 0; i < 100; i++)
                    BackgroundJob.Enqueue(() => WorkerCountTest());
            });

            app.UseMvc(routes => {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        public static int x = 0;

        [AutomaticRetry(Attempts = 2, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public static void CronTest() {
            Debug.WriteLine($"{x++} Cron Job: Hello, world!");
        }

        [Queue("testing")]
        public static void QueueTest() {
            Debug.WriteLine($"{x++} Queue test Job: Hello, world!");
        }

        [Queue("testing")]
        public static void FailingTest() {
            Debug.WriteLine($"{x++} Requeue test!");
            throw new System.Exception();
        }


        public static void WorkerCountTest() {
            Thread.Sleep(5000);
        }
    }
}
