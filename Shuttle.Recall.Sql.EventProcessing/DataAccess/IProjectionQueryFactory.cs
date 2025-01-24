using System.Collections.Generic;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public interface IProjectionQueryFactory
{
    IQuery SetSequenceNumber(string name, long sequenceNumber);
    IQuery Get(string name);
    IQuery GetIncompleteSequenceNumbers(string name);
    IQuery Complete(ProjectionEvent projectionEvent);
    IQuery RegisterJournalSequenceNumbers(string name, List<long> sequenceNumbers);
}