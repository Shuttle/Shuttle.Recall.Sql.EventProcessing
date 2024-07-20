using System;
using System.Threading.Tasks;
using System.Transactions;
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
        IPipelineObserver<OnAbortPipeline>,
        IPipelineObserver<OnAfterGetProjectionEvent>
    {
        private readonly IDatabaseContextService _databaseContextService;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;
        private readonly SqlStorageOptions _sqlStorageOptions;
        private readonly bool _isSharedConnection;

        public EventProcessingObserver(IOptions<SqlStorageOptions> sqlStorageOptions, IOptions<SqlEventProcessingOptions> eventProcessingOptions, IDatabaseContextFactory databaseContextFactory, IDatabaseContextService databaseContextService)
        {
            _databaseContextFactory = Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            _sqlStorageOptions = Guard.AgainstNull(Guard.AgainstNull(sqlStorageOptions, nameof(sqlStorageOptions)).Value, nameof(sqlStorageOptions.Value));
            _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions, nameof(eventProcessingOptions)).Value, nameof(eventProcessingOptions.Value));
            _databaseContextService = Guard.AgainstNull(databaseContextService, nameof(databaseContextService));

            _isSharedConnection = _sqlEventProcessingOptions.ConnectionStringName.Equals(_sqlStorageOptions.ConnectionStringName, StringComparison.InvariantCultureIgnoreCase);
        }

        public void Execute(OnAbortPipeline pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAbortPipeline pipelineEvent)
        {
            DisposeDatabaseContext(Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)));

            await Task.CompletedTask;
        }

        public void Execute(OnAfterGetProjectionEvent pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAfterGetProjectionEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            if (_isSharedConnection)
            {
                return;
            }

            _databaseContextService.Activate(_sqlEventProcessingOptions.ConnectionStringName);

            await Task.CompletedTask;
        }

        public void Execute(OnStageStarting pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStageStarting pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            if (!pipelineEvent.Pipeline.StageName.Equals("Process", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (_isSharedConnection)
            {
                pipelineEvent.Pipeline.State.Replace(_databaseContextFactory.Create(_sqlStorageOptions.ConnectionStringName));
            }
            else
            {
                pipelineEvent.Pipeline.State.Replace("EventProjectionDatabaseContext", _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName));

                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    pipelineEvent.Pipeline.State.Replace("EventStoreDatabaseContext", _databaseContextFactory.Create(_sqlStorageOptions.ConnectionStringName));
                }
            }

            await Task.CompletedTask;
        }

        public void Execute(OnStageCompleted pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnStageCompleted pipelineEvent)
        {
            DisposeDatabaseContext(Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)));
            
            await Task.CompletedTask;
        }

        private void DisposeDatabaseContext(PipelineEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            if (!pipelineEvent.Pipeline.StageName.Equals("Process", StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            if (_isSharedConnection)
            {
                pipelineEvent.Pipeline.State.Get<IDatabaseContext>().TryDispose();
            }
            else
            {
                pipelineEvent.Pipeline.State.Get<IDatabaseContext>("EventProjectionDatabaseContext").TryDispose();
                pipelineEvent.Pipeline.State.Get<IDatabaseContext>("EventStoreDatabaseContext").TryDispose();
            }
        }

        public void Execute(OnPipelineException pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnPipelineException pipelineEvent)
        {
            DisposeDatabaseContext(Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent)));

            await Task.CompletedTask;
        }
    }
}