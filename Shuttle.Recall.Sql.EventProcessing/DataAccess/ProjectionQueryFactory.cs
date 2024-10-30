using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public class ProjectionQueryFactory : IProjectionQueryFactory
{
    private readonly IScriptProvider _scriptProvider;
    private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;

    public ProjectionQueryFactory(IOptions<SqlEventProcessingOptions> sqlEventProcessingOptions, IScriptProvider scriptProvider)
    {
        _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(sqlEventProcessingOptions).Value);
        _scriptProvider = Guard.AgainstNull(scriptProvider);
    }

    public IQuery SetSequenceNumber(string name, long sequenceNumber)
    {
        return new Query(_scriptProvider.Get(_sqlEventProcessingOptions.ConnectionStringName, "Projection.SetSequenceNumber"))
            .AddParameter(Columns.Name, name)
            .AddParameter(Columns.SequenceNumber, sequenceNumber);
    }

    public IQuery Get(string name)
    {
        return new Query(_scriptProvider.Get(_sqlEventProcessingOptions.ConnectionStringName, "Projection.Get"))
            .AddParameter(Columns.Name, name);
    }

    public IQuery Save(Projection projection)
    {
        return new Query(_scriptProvider.Get(_sqlEventProcessingOptions.ConnectionStringName, "Projection.Save"))
            .AddParameter(Columns.Name, projection.Name)
            .AddParameter(Columns.SequenceNumber, projection.SequenceNumber);
    }

    public IQuery GetSequenceNumber(string name)
    {
        return new Query(_scriptProvider.Get(_sqlEventProcessingOptions.ConnectionStringName, "Projection.GetSequenceNumber"))
            .AddParameter(Columns.Name, name);
    }
}