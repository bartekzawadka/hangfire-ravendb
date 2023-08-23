using Hangfire.Annotations;
using Hangfire.Raven.Extensions;
using Hangfire.Raven.Storage;
using Hangfire.Storage;

namespace Hangfire.Raven.Entities
{
    public class RavenFetchedJob : IFetchedJob
    {
        private readonly RavenStorage _storage;

        private bool Requeued { get; set; }

        private bool RemovedFromQueue { get; set; }

        private bool Disposed { get; set; }

        public string Id { get; set; }

        public string JobId { get; set; }

        public string Queue { get; set; }

        public RavenFetchedJob([NotNull] RavenStorage storage, JobQueue jobQueue)
        {
            storage.ThrowIfNull("storage");
            jobQueue.ThrowIfNull("jobQueue");

            _storage = storage;

            JobId = jobQueue.JobId;
            Queue = jobQueue.Queue;
            Id = jobQueue.Id;
        }

        public void RemoveFromQueue()
        {
            using (var session = _storage.Repository.OpenSession())
            {
                var job = session.Load<JobQueue>(Id);

                if (job != null)
                {
                    session.Delete(job);
                    session.SaveChanges();
                }
            }

            RemovedFromQueue = true;
        }

        public void Requeue()
        {
            using (var repository = _storage.Repository.OpenSession())
            {
                var job = repository.Load<JobQueue>(Id);

                job.FetchedAt = null;
            }

            Requeued = true;
        }

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }

            if (!RemovedFromQueue && !Requeued)
            {
                Requeue();
            }

            Disposed = true;
        }
    }
}
