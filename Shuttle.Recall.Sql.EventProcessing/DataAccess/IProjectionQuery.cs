using System.Collections.Generic;
using System.Threading.Tasks;

namespace Shuttle.Recall.Sql.EventProcessing;

public interface IProjectionQuery
{
    Task<IEnumerable<long>> GetIncompleteSequenceNumbers(string name);
}