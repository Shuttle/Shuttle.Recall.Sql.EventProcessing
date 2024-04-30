namespace Shuttle.Recall.Sql.EventProcessing
{
    public class SqlEventProcessingOptions
    {
        public const string SectionName = "Shuttle:EventStore:Sql:EventProcessing";

        public string ConnectionStringName { get; set; }
        public bool ManageEventStoreConnections { get; set; }
    }
}