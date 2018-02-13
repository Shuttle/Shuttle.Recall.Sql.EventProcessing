using System.Configuration;
using Shuttle.Core.Configuration;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class ProjectionSection : ConfigurationSection
    {
        [ConfigurationProperty("eventStoreConnectionStringName", IsRequired = false, DefaultValue = "EventStore")]
        public string EventStoreConnectionStringName => (string) this["eventStoreConnectionStringName"];

        [ConfigurationProperty("eventProjectionConnectionStringName", IsRequired = false, DefaultValue = "EventStore")]
        public string EventProjectionConnectionStringName => (string) this["eventProjectionConnectionStringName"];

        [ConfigurationProperty("eventProjectionPrefetchCount", IsRequired = false,
            DefaultValue = ProjectionConfiguration.DefaultEventProjectionPrefetchCount)]
        public int EventProjectionPrefetchCount => (int) this["eventProjectionPrefetchCount"];

        public static ProjectionConfiguration Configuration(IConnectionConfigurationProvider provider)
        {
            Guard.AgainstNull(provider, nameof(provider));

            var section = ConfigurationSectionProvider.Open<ProjectionSection>("shuttle", "projection");
            var configuration = new ProjectionConfiguration();

            var eventStoreConnectionStringName = "EventStore";
            var eventProjectionConnectionStringName = "EventStore";

            if (section != null)
            {
                eventStoreConnectionStringName = section.EventStoreConnectionStringName;
                eventProjectionConnectionStringName = section.EventProjectionConnectionStringName;
                configuration.EventProjectionPrefetchCount = section.EventProjectionPrefetchCount;
            }

            var connectionConfiguration = provider.Get(eventStoreConnectionStringName);

            configuration.EventStoreConnectionString = connectionConfiguration.ConnectionString;
            configuration.EventStoreProviderName = connectionConfiguration.ProviderName;

            connectionConfiguration = provider.Get(eventProjectionConnectionStringName);

            configuration.EventProjectionConnectionString = connectionConfiguration.ConnectionString;
            configuration.EventProjectionProviderName = connectionConfiguration.ProviderName;

            return configuration;
        }
    }
}