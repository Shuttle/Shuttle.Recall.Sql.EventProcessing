namespace Shuttle.Recall.Sql.EventProcessing
{
	public interface IProjectionConfiguration
	{
		string EventStoreProviderName { get; set; }
		string EventStoreConnectionString { get; set; }
		string EventProjectionProviderName { get; set; }
		string EventProjectionConnectionString { get; set; }
        int EventProjectionPrefetchCount { get; set; }

        bool IsSharedConnection { get; }
	}
}