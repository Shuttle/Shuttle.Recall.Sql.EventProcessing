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
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly SqlEventProcessingOptions _eventProcessingOptions;
    private readonly IEventProcessorConfiguration _eventProcessorConfiguration;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly IPrimitiveEventQuery _primitiveEventQuery;
    private readonly IProjectionQuery _projectionQuery;
    private readonly IProjectionRepository _projectionRepository;
    private readonly Dictionary<string, List<ThreadPrimitiveEvent>> _projectionThreadPrimitiveEvents = new();
    private readonly SqlStorageOptions _storageOptions;
    private int[] _managedThreadIds = [];

    private Projection[] _projections = [];
    private int _roundRobinIndex;

    public ProjectionService(IOptions<SqlStorageOptions> storageOptions, IOptions<SqlEventProcessingOptions> eventProcessingOptions, IDatabaseContextFactory databaseContextFactory, IProjectionRepository projectionRepository, IProjectionQuery projectionQuery, IPrimitiveEventQuery primitiveEventQuery, IEventProcessorConfiguration eventProcessorConfiguration)
    {
        _storageOptions = Guard.AgainstNull(Guard.AgainstNull(storageOptions).Value);
        _eventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions).Value);
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        _projectionRepository = Guard.AgainstNull(projectionRepository);
        _projectionQuery = Guard.AgainstNull(projectionQuery);
        _primitiveEventQuery = Guard.AgainstNull(primitiveEventQuery);
        _eventProcessorConfiguration = Guard.AgainstNull(eventProcessorConfiguration);
    }

    public async Task<ProjectionEvent?> GetProjectionEventAsync(int processorThreadManagedThreadId)
    {
        Projection? projection;

        if (_projections.Length == 0)
        {
            return null;
        }

        await _lock.WaitAsync();

        try
        {
            if (_roundRobinIndex >= _projections.Length)
            {
                _roundRobinIndex = 0;
            }

            projection = _projections[_roundRobinIndex];
        }
        finally
        {
            _lock.Release();
        }

        var projectionThreadPrimitiveEvents = _projectionThreadPrimitiveEvents[projection.Name];

        if (!projectionThreadPrimitiveEvents.Any())
        {
            await GetProjectionJournalAsync(projection);
        }

        if (!projectionThreadPrimitiveEvents.Any())
        {
            return null;
        }

        var threadPrimitiveEvent = projectionThreadPrimitiveEvents.FirstOrDefault(item => item.ManagedThreadId == processorThreadManagedThreadId);

        return threadPrimitiveEvent == null ? null : new ProjectionEvent(projection, threadPrimitiveEvent.PrimitiveEvent);
    }

    public async Task AcknowledgeAsync(ProjectionEvent projectionEvent)
    {
        await using (_databaseContextFactory.Create(_eventProcessingOptions.ConnectionStringName))
        {
            await _projectionQuery.CompleteAsync(projectionEvent);
        }

        _projectionThreadPrimitiveEvents[projectionEvent.Projection.Name].RemoveAll(item => item.PrimitiveEvent.SequenceNumber == projectionEvent.PrimitiveEvent.SequenceNumber);
    }

    private async Task GetProjectionJournalAsync(Projection projection)
    {
        await _lock.WaitAsync();

        try
        {
            if (_projectionThreadPrimitiveEvents[projection.Name].Any())
            {
                return;
            }

            var journalSequenceNumbers = new List<long>();

            await using (_databaseContextFactory.Create(_storageOptions.ConnectionStringName))
            {
                var specification = new PrimitiveEventSpecification()
                    .WithMaximumRows(_eventProcessingOptions.ProjectionJournalSize)
                    .WithSequenceNumberStart(projection.SequenceNumber + 1);

                foreach (var primitiveEvent in (await _primitiveEventQuery.SearchAsync(specification)).OrderBy(item => item.SequenceNumber))
                {
                    var managedThreadId = _managedThreadIds[(primitiveEvent.CorrelationId ?? primitiveEvent.Id).GetHashCode() % _managedThreadIds.Length];

                    _projectionThreadPrimitiveEvents[projection.Name].Add(new(managedThreadId, primitiveEvent));

                    journalSequenceNumbers.Add(primitiveEvent.SequenceNumber);
                }
            }

            await using (_databaseContextFactory.Create(_eventProcessingOptions.ConnectionStringName))
            {
                await _projectionRepository.SetSequenceNumberAsync(projection.Name, await _projectionQuery.GetJournalSequenceNumberAsync(projection.Name));
                await _projectionRepository.RegisterJournalSequenceNumbersAsync(projection.Name, journalSequenceNumbers).ConfigureAwait(false);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task StartupAsync(IProcessorThreadPool processorThreadPool)
    {
        Guard.AgainstNull(processorThreadPool);

        _managedThreadIds = processorThreadPool.ProcessorThreads.Select(item => item.ManagedThreadId).ToArray();

        Dictionary<string, List<long>> incompleteSequenceNumbers = new();

        using (new DatabaseContextScope())
        {
            await using (_databaseContextFactory.Create(_eventProcessingOptions.ConnectionStringName))
            {
                List<Projection> projections = new();

                foreach (var projectionConfiguration in _eventProcessorConfiguration.Projections)
                {
                    projections.Add(await _projectionRepository.GetAsync(projectionConfiguration.Name));

                    _projectionThreadPrimitiveEvents.Add(projectionConfiguration.Name, new());

                    incompleteSequenceNumbers.Add(projectionConfiguration.Name, [..await _projectionQuery.GetIncompleteSequenceNumbers(projectionConfiguration.Name)]);
                }

                _projections = projections.ToArray();
            }

            await using (_databaseContextFactory.Create(_storageOptions.ConnectionStringName))
            {
                foreach (var pair in incompleteSequenceNumbers)
                {
                    if (pair.Value.Count == 0)
                    {
                        continue;
                    }

                    var specification = new PrimitiveEventSpecification();

                    specification.WithSequenceNumbers(pair.Value);

                    foreach (var primitiveEvent in (await _primitiveEventQuery.SearchAsync(specification)).OrderBy(item => item.SequenceNumber))
                    {
                        var managedThreadId = _managedThreadIds[(primitiveEvent.CorrelationId ?? primitiveEvent.Id).GetHashCode() % _managedThreadIds.Length];

                        _projectionThreadPrimitiveEvents[pair.Key].Add(new(managedThreadId, primitiveEvent));
                    }
                }
            }
        }
    }
}