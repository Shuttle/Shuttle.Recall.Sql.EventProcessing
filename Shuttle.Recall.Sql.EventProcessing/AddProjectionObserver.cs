using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class AddProjectionObserver : 
        IPipelineObserver<OnBeforeAddProjection>,
        IPipelineObserver<OnAfterAddProjection>
    {
        private readonly IDatabaseContextService _databaseContextService;
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;

        public AddProjectionObserver(IOptions<SqlEventProcessingOptions> eventProcessingOptions, IDatabaseContextFactory databaseContextFactory, IDatabaseContextService databaseContextService)
        {
            _databaseContextFactory = Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
            _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions, nameof(eventProcessingOptions)).Value, nameof(eventProcessingOptions.Value));
            _databaseContextService = Guard.AgainstNull(databaseContextService, nameof(databaseContextService));
        }

        public void Execute(OnBeforeAddProjection pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnBeforeAddProjection pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            pipelineEvent.Pipeline.State.Add("EventProjectionDatabaseContext", _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName));

            await Task.CompletedTask;
        }

        public void Execute(OnAfterAddProjection pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAfterAddProjection pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            pipelineEvent.Pipeline.State.Get<IDatabaseContext>("EventProjectionDatabaseContext").TryDispose();

            await Task.CompletedTask;
        }
    }
}