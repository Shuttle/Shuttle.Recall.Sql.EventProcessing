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

            return new Projection(
                Columns.Name.MapFrom(row),
                Columns.SequenceNumber.MapFrom(row),
                Columns.MachineName.MapFrom(row),
                Columns.BaseDirectory.MapFrom(row));
        }

        public void Save(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(projection));

            _databaseGateway.ExecuteUsing(_queryFactory.Save(projection));
        }

        public void SetSequenceNumber(string projectionName, long sequenceNumber)
        {
            Guard.AgainstNullOrEmptyString(projectionName, nameof(projectionName));

            _databaseGateway.ExecuteUsing(_queryFactory.SetSequenceNumber(projectionName, sequenceNumber));
        }
    }
}