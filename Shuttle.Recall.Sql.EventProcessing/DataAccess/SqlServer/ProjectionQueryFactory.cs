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
        return new Query($"update [{_sqlEventProcessingOptions.Schema}].[Projection] set SequenceNumber = @SequenceNumber where [Name] = @Name")
            .AddParameter(Columns.Name, name)
            .AddParameter(Columns.SequenceNumber, sequenceNumber);
    }

    public IQuery GetSequenceNumber(string name)
    {
        return new Query($"select SequenceNumber from [{_sqlEventProcessingOptions.Schema}].[Projection] where [Name] = @Name")
            .AddParameter(Columns.Name, name);
    }
}