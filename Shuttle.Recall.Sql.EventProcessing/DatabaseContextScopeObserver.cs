using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall.Sql.Storage
{
    public class DatabaseContextScopeObserver : 
        IPipelineObserver<OnAfterConfigureThreadPools>
    {
        public void Execute(OnAfterConfigureThreadPools pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAfterConfigureThreadPools pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            foreach (var processorThread in pipelineEvent.Pipeline.State.Get<IProcessorThreadPool>("EventProcessorThreadPool").ProcessorThreads)
            {
                processorThread.ProcessorThreadStarting += (sender, args) =>
                {
                    processorThread.SetState("DatabaseContextScope", new DatabaseContextScope());
                };

                processorThread.ProcessorThreadStopping += (sender, args) =>
                {
                    (processorThread.GetState("DatabaseContextScope") as IDisposable)?.Dispose();
                };
            }

            await Task.CompletedTask;
        }
    }
}