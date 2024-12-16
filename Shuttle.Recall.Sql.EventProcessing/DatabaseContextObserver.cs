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
    private readonly IDatabaseContextService _databaseContextService;
    private readonly IDatabaseContextFactory _databaseContextFactory;
    private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;
    private readonly SqlStorageOptions _sqlStorageOptions;

    private const string DatabaseContextStateKey = "Shuttle.Recall.Sql.EventProcessing.DatabaseContextObserver:DatabaseContext";
    private const string DisposeDatabaseContextStateKey = "Shuttle.Recall.Sql.EventProcessing.DatabaseContextObserver:DisposeDatabaseContext";

    public DatabaseContextObserver(IOptions<SqlStorageOptions> sqlStorageOptions, IOptions<SqlEventProcessingOptions> eventProcessingOptions, IDatabaseContextService databaseContextService, IDatabaseContextFactory databaseContextFactory)
    {
        _sqlStorageOptions = Guard.AgainstNull(Guard.AgainstNull(sqlStorageOptions).Value);
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
            case "EVENTPROCESSING.READ":
            {
                var hasDatabaseContext = _databaseContextService.Contains(_sqlStorageOptions.ConnectionStringName);

                pipelineContext.Pipeline.State.Add(DisposeDatabaseContextStateKey, !hasDatabaseContext);

                if (!hasDatabaseContext)
                {
                    pipelineContext.Pipeline.State.Add(DatabaseContextStateKey, _databaseContextFactory.Create(_sqlStorageOptions.ConnectionStringName));
                }

                break;
            }
            case "EVENTPROCESSING.HANDLE":
            {
                var hasDatabaseContext = _databaseContextService.Contains(_sqlEventProcessingOptions.ConnectionStringName);

                pipelineContext.Pipeline.State.Add(DisposeDatabaseContextStateKey, !hasDatabaseContext);

                if (!hasDatabaseContext)
                {
                    pipelineContext.Pipeline.State.Add(DatabaseContextStateKey, _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName));
                }


                break;
            }
        }

        await Task.CompletedTask;
    }

    private async Task DisposeDatabaseContextAsync(IPipelineContext pipelineContext)
    {
        var databaseContext = Guard.AgainstNull(pipelineContext).Pipeline.State.Get(DatabaseContextStateKey);

        if (databaseContext != null)
        {
            await databaseContext.TryDisposeAsync();
        }

        pipelineContext.Pipeline.State.Remove(DatabaseContextStateKey);
        pipelineContext.Pipeline.State.Remove(DisposeDatabaseContextStateKey);
    }
}