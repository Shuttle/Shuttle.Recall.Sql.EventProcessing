using System;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingModule
    {
        private readonly EventProcessingObserver _eventProcessingObserver;
        private readonly Type _eventProcessingPipelineType = typeof(EventProcessingPipeline);
        private readonly Type _eventProcessorStartupPipelineType = typeof(EventProcessorStartupPipeline);

        public EventProcessingModule(IPipelineFactory pipelineFactory, EventProcessingObserver eventProcessingObserver)
        {
            Guard.AgainstNull(pipelineFactory, nameof(pipelineFactory)).PipelineCreated += PipelineCreated;
            _eventProcessingObserver = Guard.AgainstNull(eventProcessingObserver, nameof(EventProcessingObserver));
        }

        private void PipelineCreated(object sender, PipelineEventArgs e)
        {
            if (e.Pipeline.GetType() != _eventProcessingPipelineType &&
                e.Pipeline.GetType() != _eventProcessorStartupPipelineType)
            {
                return;
            }

            e.Pipeline.RegisterObserver(_eventProcessingObserver);
        }
    }
}