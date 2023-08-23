using Hangfire;
using Hangfire.Raven.Samples.Web;
using Hangfire.Raven.Storage;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration(
    (context, configurationBuilder) =>
    {
        configurationBuilder
            .SetBasePath(context.HostingEnvironment.ContentRootPath)
            .AddJsonFile("appsettings.json", true, true)
            .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true)
            .AddEnvironmentVariables();
    });

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<SchedulingService>();

builder.Services.AddHangfire(
    configuration => configuration.UseRavenStorage(
        builder.Configuration["ConnectionStrings:RavenDebugUrl"],
        builder.Configuration["ConnectionStrings:RavenDebugDatabase"]));
builder.Services.AddHangfireServer(options =>
{
    options.Queues = new[] { "default", "testing" };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHangfireDashboard();

app.Run();
