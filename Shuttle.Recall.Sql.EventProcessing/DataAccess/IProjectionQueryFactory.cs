using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing
{
	public interface IProjectionQueryFactory
	{
		IQuery SetSequenceNumber(string name, long sequenceNumber);
	    IQuery GetSequenceNumber(string name);
	}
}