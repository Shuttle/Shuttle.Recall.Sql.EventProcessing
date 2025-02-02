using System.Collections.Generic;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing;

public class ProjectionQuery : IProjectionQuery
{
    private readonly IQueryMapper _queryMapper;
    private readonly IProjectionQueryFactory _queryFactory;

    public ProjectionQuery(IProjectionQueryFactory projectionQueryFactory, IQueryMapper queryMapper)
    {
        _queryFactory = Guard.AgainstNull(projectionQueryFactory);
        _queryMapper = Guard.AgainstNull(queryMapper);
    }

    public async Task<IEnumerable<long>> GetIncompleteSequenceNumbers(string name)
    {
        return await _queryMapper.MapValuesAsync<long>(_queryFactory.GetIncompleteSequenceNumbers(name));
    }
}