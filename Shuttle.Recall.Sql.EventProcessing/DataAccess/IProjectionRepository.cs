using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall.Sql.EventProcessing;

public interface IProjectionRepository
{
    Task<Projection> GetAsync(string name);
    Task SetSequenceNumberAsync(string name, long sequenceNumber);
    Task RegisterJournalSequenceNumbersAsync(string name, List<long> sequenceNumbers);
    Task CompleteAsync(ProjectionEvent projectionEvent);
}