using System.Threading.Tasks;

namespace Shuttle.Recall.Sql.EventProcessing;

public interface IProjectionQuery
{
    Task SetSequenceNumberAsync(string projectionName, long sequenceNumber);
    ValueTask<long> GetSequenceNumberAsync(string projectionName);
}