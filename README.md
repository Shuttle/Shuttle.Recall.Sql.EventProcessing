# Shuttle.Recall.Sql.EventProcessing

A Sql Server implementation of the `Shuttle.Recall` event sourcing mechanism.

### Event Sourcing / Processing

~~~ c#
// use any of the supported DI containers
var container = new WindsorComponentContainer(new WindsorContainer());

container.Register<IScriptProvider>(new ScriptProvider(new ScriptProviderConfiguration
{
	ResourceAssembly = Assembly.GetAssembly(typeof(PrimitiveEventRepository)),
	ResourceNameFormat = SqlResources.SqlClientResourceNameFormat
}));

container.Register<IDatabaseContextCache, ThreadStaticDatabaseContextCache>();
container.Register<IDatabaseContextFactory, DatabaseContextFactory>();
container.Register<IDbConnectionFactory, DbConnectionFactory>();
container.Register<IDbCommandFactory, DbCommandFactory>();
container.Register<IDatabaseGateway, DatabaseGateway>();
container.Register<IQueryMapper, QueryMapper>();
container.Register<IProjectionRepository, ProjectionRepository>();
container.Register<IProjectionQueryFactory, ProjectionQueryFactory>();
container.Register<IPrimitiveEventRepository, PrimitiveEventRepository>();
container.Register<IPrimitiveEventQueryFactory, PrimitiveEventQueryFactory>();

container.Register<IProjectionConfiguration>(ProjectionSection.Configuration());
container.Register<EventProcessingModule, EventProcessingModule>();

// register event handlers for event processing along with any other dependencies
container.Register<MyHandler, MyHandler>();
container.Register<IMyQueryFactory, MyQueryFactory>();
container.Register<IMyQuery, MyQuery>();

EventStoreConfigurator.Configure(container);

container.Resolve<EventProcessingModule>(); // resolve the event processing module to create an instance

var processor = EventProcessor.Create(container);

var projection = new Projection("name");

projection.AddEventHandler(container.Resolve<MyHandler>()); // add the handlers

processor.AddProjection(projection);

processor.Start();

Application.Run(view);

processor.Dispose();
~~~

### Application Configuration File

~~~ xml
<configuration>
	<configSections>
		<sectionGroup name="shuttle">
			<section 
				name="projection" 
				type="Shuttle.Recall.Sql.EventProcessing.ProjectionSection, Shuttle.Recall.Sql.EventProcessing" />
		</sectionGroup>
	</configSections>

	<shuttle>
		<projection connectionStringName="EventStore" />
	</shuttle>

	<connectionStrings>
		<clear />
		<add 
			name="EventStore" 
			connectionString="Data Source=.\sqlexpress;Initial Catalog=shuttle;Integrated Security=SSPI;" 
			providerName="System.Data.SqlClient" />
	</connectionStrings>
</configuration>
~~~

Use can then call `ProjectionSection.Configuration()` to return the configuration set up according to the application configuration files `ProjectionSection`.

The `IDatabaseContextFactory` and `IDatabaseGateway` implementation follow the structures as defined in the [Shuttle.Core.Data](http://shuttle.github.io/shuttle-core/overview-data/) package.

For the `IProjectionQueryFactory` you can simply specify `new ProjectionQueryFactory()`.