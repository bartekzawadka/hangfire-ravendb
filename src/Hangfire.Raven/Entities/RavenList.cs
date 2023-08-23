using System.Collections.Generic;

namespace Hangfire.Raven.Entities
{
    public class RavenList
    {
        public string Id { get; set; }

        public List<string> Values { get; set; } = new List<string>();
    }
}
