using System;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class ProjectionConfiguration : IProjectionConfiguration
    {
        public string EventStoreProviderName { get; set; }
        public string EventStoreConnectionString { get; set; }
        public string EventProjectionProviderName { get; set; }
        public string EventProjectionConnectionString { get; set; }

        public bool IsSharedConnection => EventStoreProviderName.Equals(EventProjectionProviderName,
                                            StringComparison.InvariantCultureIgnoreCase)
                                        &&
                                        EventStoreConnectionString.Equals(EventProjectionConnectionString,
                                            StringComparison.InvariantCultureIgnoreCase);
    }
}