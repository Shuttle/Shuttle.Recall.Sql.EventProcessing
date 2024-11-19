using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Sql.EventProcessing;

public class EventProcessingHostedService : IHostedService
{

    private readonly DatabaseContextObserver _databaseContextObserver;
    private readonly Type _eventProcessingPipelineType = typeof(EventProcessingPipeline);
    private readonly Type _eventProcessorStartupPipelineType = typeof(EventProcessorStartupPipeline);
    private readonly IPipelineFactory _pipelineFactory;

    public EventProcessingHostedService(IPipelineFactory pipelineFactory, DatabaseContextObserver databaseContextObserver)
    {
        _pipelineFactory = Guard.AgainstNull(pipelineFactory);
        _databaseContextObserver = Guard.AgainstNull(databaseContextObserver);

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

    private void OnPipelineCreated(object? sender, PipelineEventArgs e)
    {
        var pipelineType = e.Pipeline.GetType();

        if (pipelineType == _eventProcessingPipelineType ||
            pipelineType == _eventProcessorStartupPipelineType)
        {
            e.Pipeline.AddObserver(_databaseContextObserver);
        }

        if (pipelineType == _eventProcessorStartupPipelineType)
        {
            e.Pipeline.AddObserver<EventProcessingStartupObserver>();
        }
    }
}