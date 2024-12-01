using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Threading;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Recall.Sql.EventProcessing;

public class ProjectionService : IProjectionService
{
    private readonly IProjectionQuery _projectionQuery;
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly SqlEventProcessingOptions _eventProcessingOptions;
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IPrimitiveEventQuery _primitiveEventQuery;
    private readonly Queue<Projection> _projections = new();

    public ProjectionService(IOptions<SqlEventProcessingOptions> eventProcessingOptions, IDatabaseContextFactory databaseContextFactory, IProjectionQuery projectionQuery, IPrimitiveEventQuery primitiveEventQuery, IEventProcessorConfiguration eventProcessorConfiguration)
    {
        _eventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions).Value);
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _projectionQuery = Guard.AgainstNull(projectionQuery);
        _primitiveEventQuery = Guard.AgainstNull(primitiveEventQuery);
        _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);
    }

    public async Task<ProjectionEvent?> GetProjectionEventAsync()
    {
        Projection? projection = null;

        await _lock.WaitAsync();

        try
        {
            if (_projections.Count == 0)
            {
                return null;
            }

            projection = _projections.Dequeue();
        }
        finally
        {
            _lock.Release();
        }

        try
        {
            var projectionConfiguration = _eventProcessorConfiguration.GetProjection(projection.Name);

            var specification = new PrimitiveEventSpecification()
                .AddEventTypes(projectionConfiguration.EventTypes)
                .WithRange(projection.SequenceNumber + 1, 1)
                .WithManagedThreadId(Environment.CurrentManagedThreadId);

            var primitiveEvents = await _primitiveEventQuery.SearchAsync(specification);

            if (!primitiveEvents.Any())
            {
                await GetJournal();

                primitiveEvents = await _primitiveEventQuery.SearchAsync(specification);
            }

            if (!primitiveEvents.Any())
            {
                return null;
            }

            var primitiveEvent = primitiveEvents.FirstOrDefault();

            return primitiveEvent == null ? null : new ProjectionEvent(projection, primitiveEvent);
        }
        finally
        {
            _projections.Enqueue(projection);
        }
    }

    public async Task SetSequenceNumberAsync(string projectionName, long sequenceNumber)
    {
        //await using var databaseContext = _databaseContextFactory.Create(_eventProcessingOptions.ConnectionStringName);
        await _projectionQuery.SetSequenceNumberAsync(projectionName, sequenceNumber);
    }

    private async Task GetJournal()
    {
        await Task.CompletedTask;
    }

    public async Task StartupAsync(IProcessorThreadPool processorThreadPool)
    {
        Guard.AgainstNull(processorThreadPool);

        await using var context = _databaseContextFactory.Create(_eventProcessingOptions.ConnectionStringName);

        foreach (var projectionConfiguration in _eventProcessorConfiguration.Projections)
        {
            _projections.Enqueue(new(projectionConfiguration.Name, await _projectionQuery.GetSequenceNumberAsync(projectionConfiguration.Name)));
        }

        //  assign thread id to each existing journal entry


        await Task.CompletedTask;
    }
}