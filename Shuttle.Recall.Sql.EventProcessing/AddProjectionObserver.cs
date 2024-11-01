using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;
using Shuttle.Core.Reflection;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class AddProjectionObserver :
        IPipelineObserver<OnBeforeAddProjection>,
        IPipelineObserver<OnAfterAddProjection>,
        IPipelineObserver<OnPipelineException>,
        IPipelineObserver<OnAbortPipeline>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;

        private const string DatabaseContextStateKey = "Shuttle.Recall.Sql.EventProcessing.AddProjectionObserver:DatabaseContext";

        public AddProjectionObserver(IOptions<SqlEventProcessingOptions> eventProcessingOptions, IDatabaseContextFactory databaseContextFactory)
        {
            _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions).Value);
            _databaseContextFactory = Guard.AgainstNull(databaseContextFactory);
        }

        private async Task DisposeDatabaseContextAsync(IPipelineContext pipelineContext)
        {
            var databaseContext = Guard.AgainstNull(pipelineContext).Pipeline.State.Get(DatabaseContextStateKey);

            if (databaseContext != null)
            {
                await databaseContext.TryDisposeAsync();
            }

            pipelineContext.Pipeline.State.Remove(DatabaseContextStateKey);
        }

        public async Task ExecuteAsync(IPipelineContext<OnAfterAddProjection> pipelineContext)
        {
            await DisposeDatabaseContextAsync(pipelineContext);
        }

        public async Task ExecuteAsync(IPipelineContext<OnBeforeAddProjection> pipelineContext)
        {
            Guard.AgainstNull(pipelineContext).Pipeline.State.Add(DatabaseContextStateKey, _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName));

            await Task.CompletedTask;
        }

        public async Task ExecuteAsync(IPipelineContext<OnPipelineException> pipelineContext)
        {
            await DisposeDatabaseContextAsync(pipelineContext);
        }

        public async Task ExecuteAsync(IPipelineContext<OnAbortPipeline> pipelineContext)
        {
            await DisposeDatabaseContextAsync(pipelineContext);
        }
    }
}