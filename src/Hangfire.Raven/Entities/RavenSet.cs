using System.Collections.Generic;

namespace Hangfire.Raven.Entities
{
    public class RavenSet
    {
        public string Id { get; set; }

        public Dictionary<string, double> Scores { get; set; } = new Dictionary<string, double>();
    }
}
