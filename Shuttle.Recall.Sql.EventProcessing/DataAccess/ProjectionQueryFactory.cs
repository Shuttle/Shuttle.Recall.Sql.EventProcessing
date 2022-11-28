using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing
{
	public class ProjectionQueryFactory : IProjectionQueryFactory
	{
	    private readonly IScriptProvider _scriptProvider;

	    public ProjectionQueryFactory(IScriptProvider scriptProvider)
	    {
		    _scriptProvider = Guard.AgainstNull(scriptProvider, nameof(scriptProvider));
	    }

		public IQuery SetSequenceNumber(string name, long sequenceNumber)
		{
			return RawQuery.Create(_scriptProvider.Get("Projection.SetSequenceNumber"))
				.AddParameterValue(Columns.Name, name)
				.AddParameterValue(Columns.SequenceNumber, sequenceNumber);
		}

	    public IQuery Get(string name)
	    {
	        return RawQuery.Create(_scriptProvider.Get("Projection.Get"))
	            .AddParameterValue(Columns.Name, name);
	    }

	    public IQuery Save(Projection projection)
	    {
	        return RawQuery.Create(_scriptProvider.Get("Projection.Save"))
	            .AddParameterValue(Columns.Name, projection.Name)
	            .AddParameterValue(Columns.SequenceNumber, projection.SequenceNumber);
	    }

	    public IQuery GetSequenceNumber(string name)
	    {
		    return RawQuery.Create(_scriptProvider.Get("Projection.GetSequenceNumber"))
			    .AddParameterValue(Columns.Name, name);
	    }
	}
}