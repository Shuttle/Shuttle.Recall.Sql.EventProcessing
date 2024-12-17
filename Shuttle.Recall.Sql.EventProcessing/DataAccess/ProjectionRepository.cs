using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public class ProjectionRepository : IProjectionRepository
{
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IProjectionQueryFactory _projectionQueryFactory;

    public ProjectionRepository(IDatabaseContextService databaseContextService, IProjectionQueryFactory projectionQueryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _projectionQueryFactory = Guard.AgainstNull(projectionQueryFactory);
    }

    public async Task<Projection> GetAsync(string name)
    {
        var row = (await _databaseContextService.Active.GetRowAsync(_projectionQueryFactory.Get(name))).GuardAgainstRecordNotFound(name);

        return new(Columns.Name.Value(row)!, Columns.SequenceNumber.Value(row));
    }

    public async Task SetSequenceNumberAsync(string name, long sequenceNumber)
    {
        await _databaseContextService.Active.ExecuteAsync(_projectionQueryFactory.SetSequenceNumber(name, sequenceNumber));
    }

    public async Task RegisterJournalSequenceNumbersAsync(string name, List<long> sequenceNumbers)
    {
        await _databaseContextService.Active.ExecuteAsync(_projectionQueryFactory.RegisterJournalSequenceNumbers(name, sequenceNumbers));
    }

    public async Task CompleteAsync(ProjectionEvent projectionEvent)
    {
        await _databaseContextService.Active.ExecuteAsync(_projectionQueryFactory.Complete(projectionEvent));
    }
}