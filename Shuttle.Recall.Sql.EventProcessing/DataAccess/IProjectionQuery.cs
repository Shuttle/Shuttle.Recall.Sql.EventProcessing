using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace Shuttle.Recall.Sql.EventProcessing;

public interface IProjectionQuery
{
    Task SetSequenceNumberAsync(string name, long sequenceNumber);
    ValueTask<long> GetJournalSequenceNumberAsync(string name);
    Task<IEnumerable<long>> GetIncompleteSequenceNumbers(string name);
    Task CompleteAsync(ProjectionEvent projectionEvent);
}