using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing
{
	public interface IProjectionQueryFactory
	{
		IQuery SetSequenceNumber(string name, long sequenceNumber);
	    IQuery Get(string name);
	    IQuery Save(Projection projection);
	}
}