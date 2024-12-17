using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall.Sql.EventProcessing;

public interface IProjectionQuery
{
    ValueTask<long> GetJournalSequenceNumberAsync(string name);
    Task<IEnumerable<long>> GetIncompleteSequenceNumbers(string name);
}