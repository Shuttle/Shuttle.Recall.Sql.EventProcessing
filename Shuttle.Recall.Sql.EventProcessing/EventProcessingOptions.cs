using System;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingOptions
    {
        public string EventStoreConnectionStringName { get; set; }
        public string EventProjectionConnectionStringName { get; set; }
    }
}