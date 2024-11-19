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

    public IQuery Get(string name)
    {
        return new Query($@"
select
	[Name],
	SequenceNumber
from 
	[{_sqlEventProcessingOptions.Schema}].[Projection]
where 
	[Name] = @Name
")
            .AddParameter(Columns.Name, name);
    }

    public IQuery Save(Projection projection)
    {
        return new Query($@"
if exists
(
	select
		null
	from
		[{_sqlEventProcessingOptions.Schema}].[Projection]
	where
		[Name] = @Name
)
	update
		[{_sqlEventProcessingOptions.Schema}].[Projection]
	set
		SequenceNumber = @SequenceNumber
	where
		[Name] = @Name
else
	insert into [{_sqlEventProcessingOptions.Schema}].[Projection]
	(
		[Name],
		SequenceNumber
	)
	values
	(
		@Name,
		@SequenceNumber
	)
")
            .AddParameter(Columns.Name, projection.Name)
            .AddParameter(Columns.SequenceNumber, projection.SequenceNumber);
    }

    public IQuery GetSequenceNumber(string name)
    {
        return new Query($"select SequenceNumber from [{_sqlEventProcessingOptions.Schema}].[Projection] where [Name] = @Name")
            .AddParameter(Columns.Name, name);
    }
}