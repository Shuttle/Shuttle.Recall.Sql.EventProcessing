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