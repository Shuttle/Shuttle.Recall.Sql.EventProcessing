namespace Shuttle.Recall.Sql.EventProcessing;

public class SqlEventProcessingOptions
{
    public const string SectionName = "Shuttle:EventStore:Sql:EventProcessing";

    public string ConnectionStringName { get; set; } = string.Empty;
    public string Schema { get; set; } = "dbo";
    public int ProjectionJournalSize { get; set; } = 1000;
    public bool ConfigureDatabase { get; set; } = true;
}