using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingHostedService : IHostedService
    {
        private readonly Type _eventProcessorStartupPipeline = typeof(EventProcessorStartupPipeline);
        private readonly Type _eventProcessingPipelineType = typeof(EventProcessingPipeline);
        private readonly Type _eventProcessorStartupPipelineType = typeof(EventProcessorStartupPipeline);
        private readonly Type _addProjectionPipeline = typeof(AddProjectionPipeline);

        private readonly IDatabaseContextService _databaseContextService;
        private readonly AddProjectionObserver _addProjectionObserver;
        private readonly EventProcessingObserver _eventProcessingObserver;
        private readonly IPipelineFactory _pipelineFactory;
        private readonly SqlEventProcessingOptions _eventProcessingOptions;

        public EventProcessingHostedService(IOptions<SqlEventProcessingOptions> eventProcessingOptions, IPipelineFactory pipelineFactory, IDatabaseContextService databaseContextService, EventProcessingObserver eventProcessingObserver, AddProjectionObserver addProjectionObserver)
        {
            _eventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions, nameof(eventProcessingOptions)).Value, nameof(eventProcessingOptions.Value));
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            _databaseContextService = Guard.AgainstNull(databaseContextService, nameof(databaseContextService));
            _eventProcessingObserver = Guard.AgainstNull(eventProcessingObserver, nameof(EventProcessingObserver));
            _addProjectionObserver = Guard.AgainstNull(addProjectionObserver, nameof(addProjectionObserver));

            _pipelineFactory.PipelineCreated += OnPipelineCreated;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _pipelineFactory.PipelineCreated -= OnPipelineCreated;

            await Task.CompletedTask;
        }

        private void OnPipelineCreated(object sender, PipelineEventArgs e)
        {
            var pipelineType = e.Pipeline.GetType();

            if (pipelineType == _eventProcessorStartupPipeline)
            {
                e.Pipeline.RegisterObserver(new DatabaseContextScopeObserver());

                return;
            }

            if (pipelineType == _eventProcessingPipelineType ||
                pipelineType == _eventProcessorStartupPipelineType)
            {
                e.Pipeline.RegisterObserver(_eventProcessingObserver);
            }

            if (pipelineType == _addProjectionPipeline)
            {
                e.Pipeline.RegisterObserver(_addProjectionObserver);
            }
        }
    }
}