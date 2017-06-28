using System;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class ProjectionConfiguration : IProjectionConfiguration
    {
        public const int DefaultEventProjectionPrefetchCount = 100;

        public ProjectionConfiguration()
        {
            EventProjectionPrefetchCount = DefaultEventProjectionPrefetchCount;
        }

        public string EventStoreProviderName { get; set; }
        public string EventStoreConnectionString { get; set; }
        public string EventProjectionProviderName { get; set; }
        public string EventProjectionConnectionString { get; set; }
        public int EventProjectionPrefetchCount { get; set; }

        public bool SharedConnection
        {
            get
            {
                return EventStoreProviderName.Equals(EventProjectionProviderName,
                    StringComparison.InvariantCultureIgnoreCase)
                       &&
                       EventStoreConnectionString.Equals(EventProjectionConnectionString,
                           StringComparison.InvariantCultureIgnoreCase);
            }
        }
    }
}