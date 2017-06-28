using Shuttle.Core.Data;
using Shuttle.Core.Infrastructure;

namespace Shuttle.Recall.Sql.EventProcessing
{
	public class Bootstrap :
		IComponentRegistryBootstrap,
		IComponentResolverBootstrap
	{
		public void Register(IComponentRegistry registry)
		{
			Guard.AgainstNull(registry, "registry");

			registry.AttemptRegister<IProjectionConfiguration>(ProjectionSection.Configuration());

			registry.AttemptRegister<IScriptProviderConfiguration, ScriptProviderConfiguration>();
			registry.AttemptRegister<IScriptProvider, ScriptProvider>();

			registry.AttemptRegister<IDatabaseContextCache, ThreadStaticDatabaseContextCache>();
			registry.AttemptRegister<IDatabaseContextFactory, DatabaseContextFactory>();
			registry.AttemptRegister<IDbConnectionFactory, DbConnectionFactory>();
			registry.AttemptRegister<IDbCommandFactory, DbCommandFactory>();
			registry.AttemptRegister<IDatabaseGateway, DatabaseGateway>();
			registry.AttemptRegister<IQueryMapper, QueryMapper>();
			registry.AttemptRegister<IProjectionRepository, ProjectionRepository>();
			registry.AttemptRegister<IProjectionQueryFactory, ProjectionQueryFactory>();

			registry.AttemptRegister<EventProcessingObserver, EventProcessingObserver>();
			registry.AttemptRegister<EventProcessingModule, EventProcessingModule>();
		}

		public void Resolve(IComponentResolver resolver)
		{
			Guard.AgainstNull(resolver, "resolver");

			resolver.Resolve<EventProcessingModule>();
		}
	}
}