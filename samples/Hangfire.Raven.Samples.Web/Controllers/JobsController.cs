using Microsoft.AspNetCore.Mvc;

namespace Hangfire.Raven.Samples.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class JobsController : ControllerBase
{
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly TestJobs _testJobs;

    public JobsController(
        IRecurringJobManager recurringJobManager,
        IBackgroundJobClient backgroundJobClient,
        TestJobs testJobs)
    {
        _recurringJobManager = recurringJobManager;
        _backgroundJobClient = backgroundJobClient;
        _testJobs = testJobs;
    }

    [HttpPost("cron")]
    public IActionResult ScheduleCronJob([FromQuery] string jobId, [FromQuery] string cron)
    {
        _recurringJobManager.AddOrUpdate(jobId, () => _testJobs.CronTest(), cron);
        return Ok();
    }

    [HttpDelete("cron/{jobId}")]
    public IActionResult DeleteCronJob(string jobId)
    {
        _recurringJobManager.RemoveIfExists(jobId);
        return Ok();
    }

    [HttpPost("queue")]
    public IActionResult Enqueue()
    {
        var jobId = _backgroundJobClient.Enqueue(() => _testJobs.QueueTest());
        return Ok(jobId);
    }

    [HttpPost("delayed")]
    public IActionResult Delayed()
    {
        _backgroundJobClient.Enqueue(() => _testJobs.DelayedTest(default));
        return Ok();
    }

    [HttpPost("failing")]
    public IActionResult Failing()
    {
        _backgroundJobClient.Enqueue(() => _testJobs.FailingTest());
        return Ok();
    }
}
