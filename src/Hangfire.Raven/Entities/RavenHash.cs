using System.Collections.Generic;

namespace Hangfire.Raven.Entities
{
    public class RavenHash
    {
        public string Id { get; set; }

        public Dictionary<string, string> Fields { get; set; } = new Dictionary<string, string>();
    }
}
