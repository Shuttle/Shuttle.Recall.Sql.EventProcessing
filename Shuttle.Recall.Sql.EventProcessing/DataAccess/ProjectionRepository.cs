using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class ProjectionRepository : IProjectionRepository
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IProjectionQueryFactory _queryFactory;

        public ProjectionRepository(IDatabaseGateway databaseGateway, IProjectionQueryFactory queryFactory)
        {
            Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            Guard.AgainstNull(queryFactory, nameof(queryFactory));

            _databaseGateway = databaseGateway;
            _queryFactory = queryFactory;
        }

        public long GetSequenceNumber(string projectionName)
        {
            return _databaseGateway.GetScalarUsing<long>(_queryFactory.GetSequenceNumber(projectionName));
        }

        public void SetSequenceNumber(string projectionName, long sequenceNumber)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.SetSequenceNumber(projectionName, sequenceNumber));
        }
    }
}