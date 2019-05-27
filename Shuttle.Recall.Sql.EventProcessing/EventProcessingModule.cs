using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingModule
    {
        private readonly EventProcessingObserver _eventProcessingObserver;
        private readonly string _pipelineName = typeof(EventProcessingPipeline).FullName;

        public EventProcessingModule(IPipelineFactory pipelineFactory, EventProcessingObserver eventProcessingObserver)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory));
            Guard.AgainstNull(eventProcessingObserver, nameof(EventProcessingObserver));

            _eventProcessingObserver = eventProcessingObserver;

            pipelineFactory.PipelineCreated += PipelineCreated;
        }

        private void PipelineCreated(object sender, PipelineEventArgs e)
        {
            if (!(e.Pipeline.GetType().FullName ?? string.Empty).Equals(_pipelineName,
                StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            e.Pipeline.RegisterObserver(_eventProcessingObserver);
        }
    }
}