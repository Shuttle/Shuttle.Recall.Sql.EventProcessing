using System;
using System.Configuration;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql.EventProcessing
{
	public class ProjectionSection : ConfigurationSection
	{
		[ConfigurationProperty("eventStoreConnectionStringName", IsRequired = false, DefaultValue = "EventStore")]
		public string EventStoreConnectionStringName => (string)this["eventStoreConnectionStringName"];

	    [ConfigurationProperty("eventProjectionConnectionStringName", IsRequired = false, DefaultValue = "EventStore")]
		public string EventProjectionConnectionStringName
		{
			get { return (string)this["eventProjectionConnectionStringName"]; }
		}

		[ConfigurationProperty("eventProjectionPrefetchCount", IsRequired = false, DefaultValue = ProjectionConfiguration.DefaultEventProjectionPrefetchCount)]
		public int EventProjectionPrefetchCount
        {
			get { return (int)this["eventProjectionPrefetchCount"]; }
		}

		public static ProjectionConfiguration Configuration()
		{
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

            var settings = GetConnectionStringSettings(eventStoreConnectionStringName);

			configuration.EventStoreConnectionString = settings.ConnectionString;
			configuration.EventStoreProviderName = settings.ProviderName;

		    settings = GetConnectionStringSettings(eventProjectionConnectionStringName);

			configuration.EventProjectionConnectionString = settings.ConnectionString;
			configuration.EventProjectionProviderName = settings.ProviderName;

			return configuration;
		}

	    private static ConnectionStringSettings GetConnectionStringSettings(string connectionStringName)
	    {
            var result = ConfigurationManager.ConnectionStrings[connectionStringName];

            if (result == null)
            {
                throw new InvalidOperationException(string.Format(DataResources.ConnectionStringMissing, connectionStringName));
            }

	        return result;
	    }
	}
}