using System.Transactions;
using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingObserver :
        IPipelineObserver<OnAfterStartTransactionScope>,
        IPipelineObserver<OnAfterGetProjectionSequenceNumber>,
        IPipelineObserver<OnAfterGetProjectionPrimitiveEvent>,
        IPipelineObserver<OnDisposeTransactionScope>,
        IPipelineObserver<OnAbortPipeline>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IProjectionConfiguration _projectionConfiguration;

        public EventProcessingObserver(IDatabaseContextFactory databaseContextFactory,
            IProjectionConfiguration projectionConfiguration)
        {
            Guard.AgainstNull(databaseContextFactory, "databaseContextFactory");
            Guard.AgainstNull(projectionConfiguration, "projectionConfiguration");

            _databaseContextFactory = databaseContextFactory;
            _projectionConfiguration = projectionConfiguration;
        }

        public void Execute(OnAbortPipeline pipelineEvent)
        {
            DisposeDatabaseContext(pipelineEvent);
        }

        private static void DisposeDatabaseContext(PipelineEvent pipelineEvent)
        {
            pipelineEvent.Pipeline.State.Get<IDatabaseContext>().AttemptDispose();
        }

        public void Execute(OnAfterStartTransactionScope pipelineEvent)
        {
            if (_projectionConfiguration.SharedConnection)
            {
                pipelineEvent.Pipeline.State.Add(_databaseContextFactory.Create(_projectionConfiguration.EventStoreProviderName,
                    _projectionConfiguration.EventStoreConnectionString));
            }
            else
            {
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    pipelineEvent.Pipeline.State.Add("EventStoreDatabaseContext",
                        _databaseContextFactory.Create(_projectionConfiguration.EventStoreProviderName,
                            _projectionConfiguration.EventStoreConnectionString)
                            .WithName("EventStoreDatabaseContext"));
                }

                pipelineEvent.Pipeline.State.Add("EventProjectionDatabaseContext",
                    _databaseContextFactory.Create(_projectionConfiguration.EventProjectionProviderName,
                        _projectionConfiguration.EventProjectionConnectionString)
                        .WithName("EventProjectionDatabaseContext"));
            }
        }

        public void Execute(OnAfterGetProjectionSequenceNumber pipelineEvent)
        {
            if (_projectionConfiguration.SharedConnection)
            {
                return;
            }

            _databaseContextFactory.DatabaseContextCache.Use("EventStoreDatabaseContext");
        }

        public void Execute(OnAfterGetProjectionPrimitiveEvent pipelineEvent)
        {
            if (_projectionConfiguration.SharedConnection)
            {
                return;
            }

            _databaseContextFactory.DatabaseContextCache.Use("EventProjectionDatabaseContext");
        }

        public void Execute(OnDisposeTransactionScope pipelineEvent)
        {
            DisposeDatabaseContext(pipelineEvent);
        }
    }
}