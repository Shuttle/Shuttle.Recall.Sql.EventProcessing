using System;
using System.Transactions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;
using Shuttle.Core.PipelineTransaction;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingObserver :
        IPipelineObserver<OnAfterStartTransactionScope>,
        IPipelineObserver<OnBeforeStartEventProcessingEvent>,
        IPipelineObserver<OnAfterStartEventProcessingEvent>,
        IPipelineObserver<OnAfterGetProjectionEvent>,
        IPipelineObserver<OnDisposeTransactionScope>,
        IPipelineObserver<OnAbortPipeline>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly EventProcessingOptions _eventProcessingOptions;
        private readonly string _eventStoreProviderName;
        private readonly string _eventStoreConnectionString;
        private readonly string _eventProjectionProviderName;
        private readonly string _eventProjectionConnectionString;

        public EventProcessingObserver(IOptions<EventProcessingOptions> projectionOptions, IOptionsMonitor<ConnectionStringOptions> connectionStringOptions, IDatabaseContextFactory databaseContextFactory)
        {
            Guard.AgainstNull(projectionOptions, nameof(projectionOptions));
            Guard.AgainstNull(projectionOptions.Value, nameof(projectionOptions.Value));
            Guard.AgainstNull(connectionStringOptions, nameof(connectionStringOptions));
            Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));

            _eventProcessingOptions = projectionOptions.Value;
            _databaseContextFactory = databaseContextFactory;

            var eventStoreConnectionStringOptions =
                connectionStringOptions.Get(_eventProcessingOptions.EventStoreConnectionStringName);

            if (eventStoreConnectionStringOptions == null)
            {
                throw new InvalidOperationException(string.Format(
                    Core.Data.Resources.ConnectionStringMissingException,
                    _eventProcessingOptions.EventStoreConnectionStringName));
            }

            _eventStoreProviderName = eventStoreConnectionStringOptions.ProviderName;
            _eventStoreConnectionString = eventStoreConnectionStringOptions.ConnectionString;

            var eventProjectionConnectionStringOptions =
                connectionStringOptions.Get(_eventProcessingOptions.EventProjectionConnectionStringName);

            if (eventProjectionConnectionStringOptions == null)
            {
                throw new InvalidOperationException(string.Format(
                    Core.Data.Resources.ConnectionStringMissingException,
                    _eventProcessingOptions.EventProjectionConnectionStringName));
            }

            _eventProjectionProviderName = eventProjectionConnectionStringOptions.ProviderName;
            _eventProjectionConnectionString = eventProjectionConnectionStringOptions.ConnectionString;
        }

        public void Execute(OnAbortPipeline pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            DisposeDatabaseContext(pipelineEvent);
        }

        public void Execute(OnAfterGetProjectionEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            if (_eventProcessingOptions.IsSharedConnection())
            {
                return;
            }

            _databaseContextFactory.DatabaseContextCache.Use("EventProjectionDatabaseContext");
        }

        public void Execute(OnAfterStartTransactionScope pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            if (_eventProcessingOptions.IsSharedConnection())
            {
                pipelineEvent.Pipeline.State.Add(_databaseContextFactory.Create(_eventStoreProviderName, _eventStoreConnectionString));
            }
            else
            {
                pipelineEvent.Pipeline.State.Add("EventProjectionDatabaseContext",
                    _databaseContextFactory.Create(_eventProjectionProviderName, _eventProjectionConnectionString)
                        .WithName("EventProjectionDatabaseContext"));

                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    pipelineEvent.Pipeline.State.Add("EventStoreDatabaseContext",
                        _databaseContextFactory.Create(_eventStoreProviderName, _eventStoreConnectionString)
                            .WithName("EventStoreDatabaseContext"));
                }
            }
        }

        public void Execute(OnDisposeTransactionScope pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            DisposeDatabaseContext(pipelineEvent);
        }

        private void DisposeDatabaseContext(PipelineEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            if (_eventProcessingOptions.IsSharedConnection())
            {
                pipelineEvent.Pipeline.State.Get<IDatabaseContext>().AttemptDispose();
            }
            else
            {
                pipelineEvent.Pipeline.State.Get<IDatabaseContext>("EventProjectionDatabaseContext").AttemptDispose();
                pipelineEvent.Pipeline.State.Get<IDatabaseContext>("EventStoreDatabaseContext").AttemptDispose();
            }
        }

        public void Execute(OnAfterStartEventProcessingEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            if (_eventProcessingOptions.IsSharedConnection())
            {
                return;
            }

            _databaseContextFactory.DatabaseContextCache.Use("EventStoreDatabaseContext");
       }

        public void Execute(OnBeforeStartEventProcessingEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            if (_eventProcessingOptions.IsSharedConnection())
            {
                return;
            }

            _databaseContextFactory.DatabaseContextCache.Use("EventProjectionDatabaseContext");
        }
    }
}