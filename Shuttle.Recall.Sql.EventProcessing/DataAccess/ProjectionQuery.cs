using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public class ProjectionQuery : IProjectionQuery
{
    private readonly IQueryMapper _queryMapper;
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IProjectionQueryFactory _queryFactory;

    public ProjectionQuery(IDatabaseContextService databaseContextService, IProjectionQueryFactory queryFactory, IQueryMapper queryMapper)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _queryFactory = Guard.AgainstNull(queryFactory);
        _queryMapper = Guard.AgainstNull(queryMapper);
    }

    public async Task SetSequenceNumberAsync(string name, long sequenceNumber)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.SetSequenceNumber(Guard.AgainstNullOrEmptyString(name), sequenceNumber)).ConfigureAwait(false);
    }

    public async ValueTask<long> GetJournalSequenceNumberAsync(string name)
    {
        return await _databaseContextService.Active.GetScalarAsync<long>(_queryFactory.GetJournalSequenceNumber(name)).ConfigureAwait(false);
    }

    public async Task<IEnumerable<long>> GetIncompleteSequenceNumbers(string name)
    {
        return await _queryMapper.MapValuesAsync<long>(_queryFactory.GetIncompleteSequenceNumbers(name));
    }

    public async Task CompleteAsync(ProjectionEvent projectionEvent)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.Complete(projectionEvent));
    }
}