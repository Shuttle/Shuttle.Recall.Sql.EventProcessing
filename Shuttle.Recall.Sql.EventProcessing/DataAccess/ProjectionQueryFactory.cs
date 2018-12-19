using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing
{
	public class ProjectionQueryFactory : IProjectionQueryFactory
	{
	    private readonly IScriptProvider _scriptProvider;

	    public ProjectionQueryFactory(IScriptProvider scriptProvider)
	    {
            Guard.AgainstNull(scriptProvider, nameof(scriptProvider));

	        _scriptProvider = scriptProvider;
	    }

		public IQuery SetSequenceNumber(string name, long sequenceNumber)
		{
			return RawQuery.Create(_scriptProvider.Get("Projection.SetSequenceNumber"))
				.AddParameterValue(ProjectionPositionColumns.Name, name)
				.AddParameterValue(ProjectionPositionColumns.SequenceNumber, sequenceNumber);
		}

	    public IQuery GetSequenceNumber(string name)
	    {
	        return RawQuery.Create(_scriptProvider.Get("Projection.GetSequenceNumber"))
	            .AddParameterValue(ProjectionPositionColumns.Name, name);
	    }
    }
}