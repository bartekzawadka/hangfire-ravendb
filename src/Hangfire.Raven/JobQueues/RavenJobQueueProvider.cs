﻿using Hangfire.Annotations;
using Hangfire.Raven.Extensions;
using Hangfire.Raven.Storage;

namespace Hangfire.Raven.JobQueues
{
    public class RavenJobQueueProvider : IPersistentJobQueueProvider
    {
        private readonly IPersistentJobQueue _jobQueue;
        private readonly IPersistentJobQueueMonitoringApi _monitoringApi;

        public RavenJobQueueProvider([NotNull] RavenStorage storage, [NotNull] RavenStorageOptions options)
        {
            storage.ThrowIfNull(nameof(storage));
            options.ThrowIfNull(nameof(options));

            _jobQueue = new RavenJobQueue(storage, options);
            _monitoringApi = new RavenJobQueueMonitoringApi(storage);
        }

        public IPersistentJobQueue GetJobQueue() => _jobQueue;

        public IPersistentJobQueueMonitoringApi GetJobQueueMonitoringApi() => _monitoringApi;
    }
}
