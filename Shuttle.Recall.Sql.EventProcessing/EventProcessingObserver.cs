using System.Transactions;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingObserver :
        IPipelineObserver<OnAfterStartTransactionScope>,
        IPipelineObserver<OnAfterGetProjectionEvent>,
        IPipelineObserver<OnDisposeTransactionScope>,
        IPipelineObserver<OnAbortPipeline>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IProjectionConfiguration _projectionConfiguration;

        public EventProcessingObserver(IDatabaseContextFactory databaseContextFactory,
            IProjectionConfiguration projectionConfiguration)
        {
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            Guard.AgainstNull(projectionConfiguration, nameof(projectionConfiguration));

            _databaseContextFactory = databaseContextFactory;
            _projectionConfiguration = projectionConfiguration;
        }

        public void Execute(OnAbortPipeline pipelineEvent)
        {
            DisposeDatabaseContext(pipelineEvent);
        }

        public void Execute(OnAfterGetProjectionEvent pipelineEvent)
        {
            if (_projectionConfiguration.IsSharedConnection)
            {
                return;
            }

            _databaseContextFactory.DatabaseContextCache.Use("EventProjectionDatabaseContext");
        }

        public void Execute(OnAfterStartTransactionScope pipelineEvent)
        {
            if (_projectionConfiguration.IsSharedConnection)
            {
                pipelineEvent.Pipeline.State.Add(_databaseContextFactory.Create(
                    _projectionConfiguration.EventStoreProviderName,
                    _projectionConfiguration.EventStoreConnectionString));
            }
            else
            {
                pipelineEvent.Pipeline.State.Add("EventProjectionDatabaseContext",
                    _databaseContextFactory.Create(_projectionConfiguration.EventProjectionProviderName,
                            _projectionConfiguration.EventProjectionConnectionString)
                        .WithName("EventProjectionDatabaseContext"));

                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    pipelineEvent.Pipeline.State.Add("EventStoreDatabaseContext",
                        _databaseContextFactory.Create(_projectionConfiguration.EventStoreProviderName,
                                _projectionConfiguration.EventStoreConnectionString)
                            .WithName("EventStoreDatabaseContext"));
                }
            }
        }

        public void Execute(OnDisposeTransactionScope pipelineEvent)
        {
            DisposeDatabaseContext(pipelineEvent);
        }

        private void DisposeDatabaseContext(PipelineEvent pipelineEvent)
        {
            if (_projectionConfiguration.IsSharedConnection)
            {
                pipelineEvent.Pipeline.State.Get<IDatabaseContext>().AttemptDispose();
            }
            else
            {
                pipelineEvent.Pipeline.State.Get<IDatabaseContext>("EventProjectionDatabaseContext").AttemptDispose();
                pipelineEvent.Pipeline.State.Get<IDatabaseContext>("EventStoreDatabaseContext").AttemptDispose();
            }
        }
    }
}