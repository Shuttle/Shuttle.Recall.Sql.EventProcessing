using System;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public static class EventProcessingExtensions
    {
        public static bool IsSharedConnection(this EventProcessingOptions eventProcessingOptions)
        {
            Guard.AgainstNull(eventProcessingOptions, nameof(eventProcessingOptions));

            return eventProcessingOptions.EventProjectionConnectionStringName.Equals(eventProcessingOptions.EventStoreConnectionStringName, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}