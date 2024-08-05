using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingObserver :
        IPipelineObserver<OnStageStarting>,
        IPipelineObserver<OnStageCompleted>,
        IPipelineObserver<OnPipelineException>,
        IPipelineObserver<OnAbortPipeline>
    {
        private readonly IDatabaseContextService _databaseContextService;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;
        private readonly SqlStorageOptions _sqlStorageOptions;

        private const string DatabaseContextStateKey = "Shuttle.Recall.Sql.EventProcessing.EventProcessingObserver:DatabaseContext";
        private const string DisposeDatabaseContextStateKey = "Shuttle.Recall.Sql.EventProcessing.EventProcessingObserver:DisposeDatabaseContext";

        public EventProcessingObserver(IOptions<SqlStorageOptions> sqlStorageOptions, IOptions<SqlEventProcessingOptions> eventProcessingOptions, IDatabaseContextService databaseContextService, IDatabaseContextFactory databaseContextFactory)
        {
            _sqlStorageOptions = Guard.AgainstNull(Guard.AgainstNull(sqlStorageOptions, nameof(sqlStorageOptions)).Value, nameof(sqlStorageOptions.Value));
            _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions, nameof(eventProcessingOptions)).Value, nameof(eventProcessingOptions.Value));
            _databaseContextService = Guard.AgainstNull(databaseContextService, nameof(databaseContextService));
            _databaseContextFactory = Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
        }

        public void Execute(OnAbortPipeline pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAbortPipeline pipelineEvent)
        {
            await DisposeDatabaseContextAsync(Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)));
        }

        public void Execute(OnPipelineException pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnPipelineException pipelineEvent)
        {
            await DisposeDatabaseContextAsync(Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)));
        }

        public void Execute(OnStageCompleted pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStageCompleted pipelineEvent)
        {
            await DisposeDatabaseContextAsync(Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)));
        }

        public void Execute(OnStageStarting pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStageStarting pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            switch (pipelineEvent.Pipeline.StageName.ToUpperInvariant())
            {
                case "EVENTPROCESSING.READ":
                {
                    var hasDatabaseContext = _databaseContextService.Contains(_sqlStorageOptions.ConnectionStringName);

                    pipelineEvent.Pipeline.State.Add(DisposeDatabaseContextStateKey, !hasDatabaseContext);

                    if (!hasDatabaseContext)
                    {
                        pipelineEvent.Pipeline.State.Add(DatabaseContextStateKey, _databaseContextFactory.Create(_sqlStorageOptions.ConnectionStringName));
                    }

                    break;
                }
                case "EVENTPROCESSING.HANDLE":
                {
                    var hasDatabaseContext = _databaseContextService.Contains(_sqlEventProcessingOptions.ConnectionStringName);

                    pipelineEvent.Pipeline.State.Add(DisposeDatabaseContextStateKey, !hasDatabaseContext);

                    if (!hasDatabaseContext)
                    {
                        pipelineEvent.Pipeline.State.Add(DatabaseContextStateKey, _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName));
                    }


                    break;
                }
            }

            await Task.CompletedTask;
        }

        private async Task DisposeDatabaseContextAsync(PipelineEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            var databaseContext = pipelineEvent.Pipeline.State.Get(DatabaseContextStateKey);

            if (databaseContext != null)
            {
                await databaseContext.TryDisposeAsync();
            }

            pipelineEvent.Pipeline.State.Remove(DatabaseContextStateKey);
            pipelineEvent.Pipeline.State.Remove(DisposeDatabaseContextStateKey);
        }
    }
}