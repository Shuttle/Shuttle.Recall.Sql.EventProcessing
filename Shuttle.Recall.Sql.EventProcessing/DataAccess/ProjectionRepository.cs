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

        public Projection Find(string name)
        {
            var row = _databaseGateway.GetSingleRowUsing(_queryFactory.Get(name));

            if (row == null)
            {
                return null;
            }

            return new Projection();
        }

        public void Save(Projection projection)
        {
            throw new System.NotImplementedException();
        }

        public void SetSequenceNumber(string projectionName, long sequenceNumber)
        {
            _databaseGateway.ExecuteUsing(_queryFactory.SetSequenceNumber(projectionName, sequenceNumber));
        }
    }
}