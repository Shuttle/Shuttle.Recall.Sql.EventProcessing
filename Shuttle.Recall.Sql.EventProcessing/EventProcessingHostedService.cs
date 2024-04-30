using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingHostedService : IHostedService

    {
        private readonly EventProcessingObserver _eventProcessingObserver;
        private readonly Type _eventProcessingPipelineType = typeof(EventProcessingPipeline);
        private readonly Type _eventProcessorStartupPipelineType = typeof(EventProcessorStartupPipeline);
        private readonly IPipelineFactory _pipelineFactory;

        public EventProcessingHostedService(IPipelineFactory pipelineFactory, EventProcessingObserver eventProcessingObserver)
        {
            _pipelineFactory = Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            _eventProcessingObserver = Guard.AgainstNull(eventProcessingObserver, nameof(EventProcessingObserver));

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
            if (e.Pipeline.GetType() == _eventProcessingPipelineType ||
                e.Pipeline.GetType() == _eventProcessorStartupPipelineType)
            {
                e.Pipeline.RegisterObserver(_eventProcessingObserver);
            }
        }
    }
}