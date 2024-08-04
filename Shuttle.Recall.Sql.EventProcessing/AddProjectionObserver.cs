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
            _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions, nameof(eventProcessingOptions)).Value, nameof(eventProcessingOptions.Value));
            _databaseContextFactory = Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
        }

        public void Execute(OnAfterAddProjection pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        private async Task DisposeDatabaseContextAsync(PipelineEvent pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            var databaseContext = pipelineEvent.Pipeline.State.Get(DatabaseContextStateKey);

            if (databaseContext != null)
            {
                await databaseContext.TryDisposeAsync();
            }

            pipelineEvent.Pipeline.State.Remove(DatabaseContextStateKey);
        }

        public async Task ExecuteAsync(OnAfterAddProjection pipelineEvent)
        {
            await DisposeDatabaseContextAsync(pipelineEvent);
        }

        public void Execute(OnBeforeAddProjection pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnBeforeAddProjection pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            pipelineEvent.Pipeline.State.Add(DatabaseContextStateKey, _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName));

            await Task.CompletedTask;
        }

        public void Execute(OnPipelineException pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnPipelineException pipelineEvent)
        {
            await DisposeDatabaseContextAsync(pipelineEvent);
        }

        public void Execute(OnAbortPipeline pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAbortPipeline pipelineEvent)
        {
            await DisposeDatabaseContextAsync(pipelineEvent);
        }
    }
}