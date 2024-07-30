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
        IPipelineObserver<OnAfterAddProjection>
    {
        private readonly IDatabaseContextFactory _databaseContextFactory;
        private readonly IDatabaseContextService _databaseContextService;
        private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;

        private const string DatabaseContextStateKey = "Shuttle.Recall.Sql.EventProcessing.AddProjectionObserver:DatabaseContext";
        private const string CloseConnectionStateKey = "Shuttle.Recall.Sql.EventProcessing.AddProjectionObserver:CloseConnection";

        public AddProjectionObserver(IOptions<SqlEventProcessingOptions> eventProcessingOptions, IDatabaseContextService databaseContextService, IDatabaseContextFactory databaseContextFactory)
        {
            _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(eventProcessingOptions, nameof(eventProcessingOptions)).Value, nameof(eventProcessingOptions.Value));
            _databaseContextService = Guard.AgainstNull(databaseContextService, nameof(databaseContextService));
            _databaseContextFactory = Guard.AgainstNull(databaseContextFactory, nameof(databaseContextFactory));
        }

        public void Execute(OnAfterAddProjection pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnAfterAddProjection pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            if (pipelineEvent.Pipeline.State.Get<bool>(CloseConnectionStateKey))
            {
                await pipelineEvent.Pipeline.State.Get<IDatabaseContext>(DatabaseContextStateKey).TryDisposeAsync();
            }

            pipelineEvent.Pipeline.State.Remove(DatabaseContextStateKey);
            pipelineEvent.Pipeline.State.Remove(CloseConnectionStateKey);
        }

        public void Execute(OnBeforeAddProjection pipelineEvent)
        {
            ExecuteAsync(pipelineEvent).GetAwaiter().GetResult();
        }

        public async Task ExecuteAsync(OnBeforeAddProjection pipelineEvent)
        {
            Guard.AgainstNull(pipelineEvent, nameof(pipelineEvent));

            var hasExistingConnection = _databaseContextService.Contains(_sqlEventProcessingOptions.ConnectionStringName);

            pipelineEvent.Pipeline.State.Add(CloseConnectionStateKey, !hasExistingConnection);

            if (!hasExistingConnection)
            {
                pipelineEvent.Pipeline.State.Add(DatabaseContextStateKey, _databaseContextFactory.Create(_sqlEventProcessingOptions.ConnectionStringName));
            }

            await Task.CompletedTask;
        }
    }
}