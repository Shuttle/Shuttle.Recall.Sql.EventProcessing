using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class ProjectionRepository : IProjectionRepository
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly EventStoreOptions _eventStoreOptions;
        private readonly IProjectionQueryFactory _queryFactory;

        public ProjectionRepository(IOptions<EventStoreOptions> eventStoreOptions, IDatabaseGateway databaseGateway, IProjectionQueryFactory queryFactory)
        {
            _eventStoreOptions = Guard.AgainstNull(Guard.AgainstNull(eventStoreOptions, nameof(eventStoreOptions)).Value, nameof(eventStoreOptions.Value));
            _databaseGateway = Guard.AgainstNull(databaseGateway, nameof(databaseGateway));
            _queryFactory = Guard.AgainstNull(queryFactory, nameof(queryFactory));
        }

        public Projection Find(string name)
        {
            return FindAsync(name, true).GetAwaiter().GetResult();
        }

        public async Task<Projection> FindAsync(string name)
        {
            return await FindAsync(name, false).ConfigureAwait(false);
        }

        public void Save(Projection projection)
        {
            SaveAsync(projection, true).GetAwaiter().GetResult();
        }

        public async Task SaveAsync(Projection projection)
        {
            await SaveAsync(projection, false).ConfigureAwait(false);
        }

        public void SetSequenceNumber(string projectionName, long sequenceNumber)
        {
            SetSequenceNumberAsync(projectionName, sequenceNumber, true).GetAwaiter().GetResult();
        }

        public async Task SetSequenceNumberAsync(string projectionName, long sequenceNumber)
        {
            await SetSequenceNumberAsync(projectionName, sequenceNumber, false).ConfigureAwait(false);
        }

        public long GetSequenceNumber(string projectionName)
        {
            return GetSequenceNumberAsync(projectionName, true).GetAwaiter().GetResult();
        }

        public async ValueTask<long> GetSequenceNumberAsync(string projectionName)
        {
            return await GetSequenceNumberAsync(projectionName, false).ConfigureAwait(false);
        }

        private async Task<Projection> FindAsync(string name, bool sync)
        {
            var row = sync
                ? _databaseGateway.GetRow(_queryFactory.Get(name))
                : await _databaseGateway.GetRowAsync(_queryFactory.Get(name));

            if (row == null)
            {
                return null;
            }

            return new Projection(_eventStoreOptions, Columns.Name.Value(row), Columns.SequenceNumber.Value(row));
        }

        private async ValueTask<long> GetSequenceNumberAsync(string projectionName, bool sync)
        {
            return (
                sync
                    ? _databaseGateway.GetScalar<long?>(_queryFactory.GetSequenceNumber(projectionName))
                    : await _databaseGateway.GetScalarAsync<long?>(_queryFactory.GetSequenceNumber(projectionName)).ConfigureAwait(false)
            ) ?? 0;
        }

        private async Task SaveAsync(Projection projection, bool sync)
        {
            Guard.AgainstNull(projection, nameof(projection));

            if (sync)
            {
                _databaseGateway.Execute(_queryFactory.Save(projection));
            }
            else
            {
                await _databaseGateway.ExecuteAsync(_queryFactory.Save(projection)).ConfigureAwait(false);
            }
        }

        private async Task SetSequenceNumberAsync(string projectionName, long sequenceNumber, bool sync)
        {
            Guard.AgainstNullOrEmptyString(projectionName, nameof(projectionName));

            if (sync)
            {
                _databaseGateway.Execute(_queryFactory.SetSequenceNumber(projectionName, sequenceNumber));
            }
            else
            {
                await _databaseGateway.ExecuteAsync(_queryFactory.SetSequenceNumber(projectionName, sequenceNumber)).ConfigureAwait(false);
            }
        }
    }
}