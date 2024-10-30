using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public interface IProjectionQueryFactory
{
    IQuery Get(string name);
    IQuery GetSequenceNumber(string name);
    IQuery Save(Projection projection);
    IQuery SetSequenceNumber(string name, long sequenceNumber);
}