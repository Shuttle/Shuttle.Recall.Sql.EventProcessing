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
            _databaseGateway = Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            _queryFactory = Guard.AgainstNull(queryFactory, nameof(queryFactory));
        }

        public Projection Find(string name)
        {
            var row = _databaseGateway.GetRow(_queryFactory.Get(name));

            if (row == null)
            {
                return null;
            }

            return new Projection(
                Columns.Name.MapFrom(row),
                Columns.SequenceNumber.MapFrom(row));
        }

        public void Save(Projection projection)
        {
            Guard.AgainstNull(projection, nameof(projection));

            _databaseGateway.Execute(_queryFactory.Save(projection));
        }

        public void SetSequenceNumber(string projectionName, long sequenceNumber)
        {
            Guard.AgainstNullOrEmptyString(projectionName, nameof(projectionName));

            _databaseGateway.Execute(_queryFactory.SetSequenceNumber(projectionName, sequenceNumber));
        }

        public long GetSequenceNumber(string projectionName)
        {
            return _databaseGateway.GetScalar<long?>(_queryFactory.GetSequenceNumber(projectionName)) ?? 0;
        }
    }
}