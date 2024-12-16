using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing.SqlServer;

public class ProjectionQueryFactory : IProjectionQueryFactory
{
    private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;

    public ProjectionQueryFactory(IOptions<SqlEventProcessingOptions> sqlEventProcessingOptions)
    {
        _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(sqlEventProcessingOptions).Value);
    }

    public IQuery SetSequenceNumber(string name, long sequenceNumber)
    {
        return new Query($"UPDATE [{_sqlEventProcessingOptions.Schema}].[Projection] SET SequenceNumber = @SequenceNumber WHERE [Name] = @Name")
            .AddParameter(Columns.Name, name)
            .AddParameter(Columns.SequenceNumber, sequenceNumber);
    }

    public IQuery Get(string name)
    {
        return new Query($@"
IF NOT EXISTS (SELECT NULL FROM [{_sqlEventProcessingOptions.Schema}].[Projection] WHERE [Name] = @Name)
BEGIN
    INSERT INTO [{_sqlEventProcessingOptions.Schema}].[Projection] 
    (
        [Name], 
        [SequenceNumber]
    ) 
    VALUES 
    (
        @Name, 
        0
    )
END

SELECT 
    [Name], 
    [SequenceNumber]
FROM 
    [{_sqlEventProcessingOptions.Schema}].[Projection] 
WHERE 
    [Name] = @Name
")
            .AddParameter(Columns.Name, name);
    }

    public IQuery Save(Projection projection)
    {
        Guard.AgainstNull(projection);

        return new Query(@$"
IF NOT EXISTS (SELECT NULL FROM [{_sqlEventProcessingOptions.Schema}].[Projection] WHERE [Name] = @Name)
BEGIN
    INSERT INTO [{_sqlEventProcessingOptions.Schema}].[Projection] 
    (
        [Name], 
        [SequenceNumber]
    ) 
    VALUES 
    (
        @Name, 
        0
    )
END
ELSE
    UPDATE 
        [{_sqlEventProcessingOptions.Schema}].[Projection] 
    SET 
        SequenceNumber = @SequenceNumber 
    WHERE 
        [Name] = @Name
")
            .AddParameter(Columns.Name, projection.Name)
            .AddParameter(Columns.SequenceNumber, projection.SequenceNumber);
    }

    public IQuery GetIncompleteSequenceNumbers(string name)
    {
        return new Query($@"
SELECT
    [SequenceNumber]
FROM
    [{_sqlEventProcessingOptions.Schema}].[ProjectionJournal]
WHERE
    [Name] = @Name
AND
    [DateCompleted] IS NULL
")
            .AddParameter(Columns.Name, name);
    }

    public IQuery Complete(ProjectionEvent projectionEvent)
    {
        return new Query($@"
UPDATE
    [{_sqlEventProcessingOptions.Schema}].[ProjectionJournal]
SET
    DateCompleted = GETUTCDATE()
WHERE
    Name = @Name
AND 
    SequenceNumber = @SequenceNumber
")
            .AddParameter(Columns.Name, projectionEvent.Projection.Name)
            .AddParameter(Columns.SequenceNumber, projectionEvent.PrimitiveEvent.SequenceNumber);
    }

    public IQuery RegisterJournalSequenceNumbers(string name, List<long> sequenceNumbers)
    {
        var sql = new StringBuilder($@"
DELETE
FROM
    [{_sqlEventProcessingOptions.Schema}].[ProjectionJournal]
WHERE
    [Name] = @Name;
");

        if (sequenceNumbers.Any())
        {
            sql.Append($@"
INSERT INTO 
    [{_sqlEventProcessingOptions.Schema}].[ProjectionJournal]
(
    [Name],
    [SequenceNumber]
)
VALUES
    {string.Join(",", sequenceNumbers.Select(sequenceNumber => $"(@Name, {sequenceNumber})"))} 
");
        }
        return new Query(sql.ToString()).AddParameter(Columns.Name, name);
    }

    public IQuery GetJournalSequenceNumber(string name)
    {
        return new Query($@"
SELECT 
    ISNULL(MAX(SequenceNumber), 0)
FROM 
    [{_sqlEventProcessingOptions.Schema}].[ProjectionJournal] 
WHERE 
    [Name] = @Name
")
            .AddParameter(Columns.Name, name);
    }
}