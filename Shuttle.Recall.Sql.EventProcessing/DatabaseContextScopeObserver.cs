using System;
using System.Diagnostics;
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

            pipelineEvent.Pipeline.State.Get<IProcessorThreadPool>("EventProcessorThreadPool").ProcessorThreadCreated += (sender, args) =>
            {
                args.ProcessorThread.ProcessorThreadStarting += (processorThreadSender, processorThreadArgs) =>
                {
                    args.ProcessorThread.SetState("DatabaseContextScope", new DatabaseContextScope());
                };

                args.ProcessorThread.ProcessorThreadStopping += (processorThreadSender, processorThreadArgs) =>
                {
                    (args.ProcessorThread.GetState("DatabaseContextScope") as IDisposable)?.Dispose();
                };
            };

            await Task.CompletedTask;
        }
    }
}