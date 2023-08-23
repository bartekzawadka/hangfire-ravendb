using System;
using System.Collections.Generic;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;

namespace Hangfire.Raven.Entities
{
    public class RavenJob
    {
        public string Id { get; set; }

        public InvocationData InvocationData { get; set; }

        public IDictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        public DateTime CreatedAt { get; set; }

        public StateData StateData { get; set; }

        public List<StateHistoryDto> History { get; set; } = new List<StateHistoryDto>();
    }
}
