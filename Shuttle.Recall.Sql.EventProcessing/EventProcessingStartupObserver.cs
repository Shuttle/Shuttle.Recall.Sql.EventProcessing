using System;
using System.Threading.Tasks;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Threading;

namespace Shuttle.Recall.Sql.EventProcessing;

public class EventProcessingStartupObserver : IPipelineObserver<OnAfterConfigureThreadPools>
{
    private readonly IProjectionEventProvider _projectionEventProvider;

    public EventProcessingStartupObserver(IProjectionEventProvider projectionEventProvider)
    {
        _projectionEventProvider = Guard.AgainstNull(projectionEventProvider);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAfterConfigureThreadPools> pipelineContext)
    {
        if (_projectionEventProvider is not ProjectionEventProvider provider)
        {
            throw new InvalidOperationException(string.Format(Resources.ProjectionEventProviderTypeException, _projectionEventProvider.GetType().FullName));
        }

        await provider.StartupAsync(Guard.AgainstNull(Guard.AgainstNull(pipelineContext).Pipeline.State.Get<IProcessorThreadPool>("EventProcessorThreadPool")));
    }
}