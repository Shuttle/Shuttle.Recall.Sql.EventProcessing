using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public class ProjectionQuery : IProjectionQuery
{
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IProjectionQueryFactory _queryFactory;

    public ProjectionQuery(IDatabaseContextService databaseContextService, IProjectionQueryFactory queryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _queryFactory = Guard.AgainstNull(queryFactory);
    }

    public async Task SetSequenceNumberAsync(string projectionName, long sequenceNumber)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.SetSequenceNumber(Guard.AgainstNullOrEmptyString(projectionName), sequenceNumber)).ConfigureAwait(false);
    }

    public async ValueTask<long> GetSequenceNumberAsync(string projectionName)
    {
        return await _databaseContextService.Active.GetScalarAsync<long?>(_queryFactory.GetSequenceNumber(projectionName)).ConfigureAwait(false) ?? 0;
    }
}