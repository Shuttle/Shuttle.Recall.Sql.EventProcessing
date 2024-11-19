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

public class ProjectionEventProvider : IProjectionEventProvider
{
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly SqlEventProcessingOptions _eventProcessingOptions;
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IPrimitiveEventQuery _primitiveEventQuery;
    private readonly IProjectionRepository _projectionRepository;
    private readonly Queue<Projection> _projections = new();
    private readonly SqlStorageOptions _storageOptions;

    public ProjectionEventProvider(IOptions<SqlStorageOptions> storageOptions, IOptions<SqlEventProcessingOptions> eventProcessingOptions, IDatabaseContextFactory databaseContextFactory, IProjectionRepository projectionRepository, IPrimitiveEventQuery primitiveEventQuery, IEventProcessorConfiguration eventProcessorConfiguration)
    {
        _storageOptions = Guard.AgainstNull(Guard.AgainstNull(storageOptions).Value);
        _eventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions).Value);
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _projectionRepository = Guard.AgainstNull(projectionRepository);
        _primitiveEventQuery = Guard.AgainstNull(primitiveEventQuery);
        _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);
    }

    public async Task<ProjectionEvent?> GetAsync()
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
            var projection = await _projectionRepository.FindAsync(projectionConfiguration.Name);

            if (projection == null)
            {
                projection = new(projectionConfiguration.Name, 0);

                await _projectionRepository.SaveAsync(projection);
            }

            _projections.Enqueue(projection);
        }

        //  assign thread id to each existing journal entry


        await Task.CompletedTask;
    }
}