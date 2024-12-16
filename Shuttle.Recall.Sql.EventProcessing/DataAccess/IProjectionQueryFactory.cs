using System.Collections.Generic;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public interface IProjectionQueryFactory
{
    IQuery GetJournalSequenceNumber(string name);
    IQuery SetSequenceNumber(string name, long sequenceNumber);
    IQuery Get(string name);
    IQuery Save(Projection projection);
    IQuery GetIncompleteSequenceNumbers(string name);
    IQuery Complete(ProjectionEvent projectionEvent);
    IQuery RegisterJournalSequenceNumbers(string name, List<long> sequenceNumbers);
}