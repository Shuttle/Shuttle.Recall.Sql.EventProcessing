using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public class ProjectionRepository : IProjectionRepository
{
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly IProjectionQueryFactory _queryFactory;
    private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;

    public ProjectionRepository(IOptions<SqlEventProcessingOptions> sqlEventProcessingOptions, IDatabaseContextFactory databaseContextFactory, IProjectionQueryFactory queryFactory)
    {
        _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(sqlEventProcessingOptions).Value);
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _queryFactory = Guard.AgainstNull(queryFactory);
    }

    public async Task<Projection?> FindAsync(string name)
    {
        await using var databaseContext = _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName);

        var row = await databaseContext.GetRowAsync(_queryFactory.Get(name));

        if (row == null)
        {
            return null;
        }

        return new(Guard.AgainstNullOrEmptyString(Columns.Name.Value(row)), Columns.SequenceNumber.Value(row));
    }

    public async ValueTask<long> GetSequenceNumberAsync(string projectionName)
    {
        await using var databaseContext = _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName);
        return await databaseContext.GetScalarAsync<long?>(_queryFactory.GetSequenceNumber(projectionName)).ConfigureAwait(false) ?? 0;
    }

    public async Task SaveAsync(Projection projection)
    {
        Guard.AgainstNull(projection);

        await using var databaseContext = _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName);
        await databaseContext.ExecuteAsync(_queryFactory.Save(projection)).ConfigureAwait(false);
    }

    public async Task SetSequenceNumberAsync(string projectionName, long sequenceNumber)
    {
        Guard.AgainstNullOrEmptyString(projectionName);

        await using var databaseContext = _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName);
        await databaseContext.ExecuteAsync(_queryFactory.SetSequenceNumber(projectionName, sequenceNumber)).ConfigureAwait(false);
    }
}