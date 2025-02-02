using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Recall.Sql.EventProcessing;

public class DatabaseContextObserver :
    IPipelineObserver<OnStageStarting>,
    IPipelineObserver<OnStageCompleted>,
    IPipelineObserver<OnPipelineException>,
    IPipelineObserver<OnAbortPipeline>
{
    private const string EventProcessingDatabaseContextStateKey = "Shuttle.Recall.Sql.EventProcessing.DatabaseContextObserver:EventProcessingDatabaseContext";

    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly IDatabaseContextService _databaseContextService;
    private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;

    public DatabaseContextObserver(IOptions<SqlEventProcessingOptions> eventProcessingOptions, IDatabaseContextService databaseContextService, IDatabaseContextFactory databaseContextFactory)
    {
        _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions.Value));
        _databaseContextService = Guard.AgainstNull(databaseContextService);
        _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
    }

    public async Task ExecuteAsync(IPipelineContext<OnAbortPipeline> pipelineContext)
    {
        await DisposeDatabaseContextAsync(Guard.AgainstNull(pipelineContext));
    }

    public async Task ExecuteAsync(IPipelineContext<OnPipelineException> pipelineContext)
    {
        await DisposeDatabaseContextAsync(Guard.AgainstNull(pipelineContext));
    }

    public async Task ExecuteAsync(IPipelineContext<OnStageCompleted> pipelineContext)
    {
        await DisposeDatabaseContextAsync(Guard.AgainstNull(pipelineContext));
    }

    public async Task ExecuteAsync(IPipelineContext<OnStageStarting> pipelineContext)
    {
        switch (Guard.AgainstNull(pipelineContext).Pipeline.StageName.ToUpperInvariant())
        {
            case "HANDLE":
            {
                pipelineContext.Pipeline.State.Replace(EventProcessingDatabaseContextStateKey,
                    !_databaseContextService.Contains(_sqlEventProcessingOptions.ConnectionStringName)
                        ? _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName)
                        : null);

                break;
            }
        }

        await Task.CompletedTask;
    }

    private async Task DisposeDatabaseContextAsync(IPipelineContext pipelineContext)
    {
        await (Guard.AgainstNull(pipelineContext).Pipeline.State.Get(EventProcessingDatabaseContextStateKey)?.TryDisposeAsync() ?? Task.CompletedTask);

        pipelineContext.Pipeline.State.Remove(EventProcessingDatabaseContextStateKey);
    }
}