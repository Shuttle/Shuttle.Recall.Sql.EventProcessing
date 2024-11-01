using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public class ProjectionRepository : IProjectionRepository
{
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IProjectionQueryFactory _queryFactory;

    public ProjectionRepository(IDatabaseContextService databaseContextService, IProjectionQueryFactory queryFactory)
    {
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _queryFactory = Guard.AgainstNull(queryFactory);
    }

    public async Task<Projection?> FindAsync(string name)
    {
        var row = await _databaseContextService.Active.GetRowAsync(_queryFactory.Get(Guard.AgainstNullOrEmptyString(name)));

        if (row == null)
        {
            return null;
        }

        return new(Guard.AgainstNullOrEmptyString(Columns.Name.Value(row)), Columns.SequenceNumber.Value(row));
    }

    public async ValueTask<long> GetSequenceNumberAsync(string projectionName)
    {
        return await _databaseContextService.Active.GetScalarAsync<long?>(_queryFactory.GetSequenceNumber(projectionName)).ConfigureAwait(false) ?? 0;
    }

    public async Task SaveAsync(Projection projection)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.Save(Guard.AgainstNull(projection))).ConfigureAwait(false);
    }

    public async Task SetSequenceNumberAsync(string projectionName, long sequenceNumber)
    {
        await _databaseContextService.Active.ExecuteAsync(_queryFactory.SetSequenceNumber(Guard.AgainstNullOrEmptyString(projectionName), sequenceNumber)).ConfigureAwait(false);
    }
}