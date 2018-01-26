using Moq;
using NUnit.Framework;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Recall.Sql.Storage;
using Shuttle.Recall.Tests;
#if (NETCOREAPP2_0 || NETSTANDARD2_0)
using Shuttle.Core.Data.SqlClient;

#endif

namespace Shuttle.Recall.Sql.EventProcessing.Tests
{
    [TestFixture]
    public class Fixture
    {
        [SetUp]
        public void TestSetUp()
        {
            DatabaseGateway = new DatabaseGateway();
            DatabaseContextCache = new ThreadStaticDatabaseContextCache();

#if (!NETCOREAPP2_0 && !NETSTANDARD2_0)
            DatabaseContextFactory = new DatabaseContextFactory(
                new ConnectionConfigurationProvider(),
                new DbConnectionFactory(), 
                new DbCommandFactory(), 
                new ThreadStaticDatabaseContextCache());
#else
            var mockConnectionConfigurationProvider = new Mock<IConnectionConfigurationProvider>();

            mockConnectionConfigurationProvider.Setup(m => m.Get(It.IsAny<string>())).Returns(
                (string name) =>
                    name.Equals("EventStoreProjection")
                        ? new ConnectionConfiguration(
                            "EventStoreProjection",
                            "System.Data.SqlClient",
                            "Data Source=.\\sqlexpress;Initial Catalog=ShuttleProjection;Integrated Security=SSPI;")
                        : new ConnectionConfiguration(
                            name,
                            "System.Data.SqlClient",
                            "Data Source=.\\sqlexpress;Initial Catalog=Shuttle;Integrated Security=SSPI;"));

            ConnectionConfigurationProvider = mockConnectionConfigurationProvider.Object;

            DatabaseContextFactory = new DatabaseContextFactory(
                ConnectionConfigurationProvider,
                new DbConnectionFactory(new DbProviderFactories()),
                new DbCommandFactory(),
                new ThreadStaticDatabaseContextCache());
#endif

            ClearDataStore();
        }

        [TearDown]
        protected void ClearDataStore()
        {
            using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
            {
                DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from EventStore where Id = @Id")
                    .AddParameterValue(EventStoreColumns.Id, RecallFixture.OrderId));
                DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from EventStore where Id = @Id")
                    .AddParameterValue(EventStoreColumns.Id, RecallFixture.OrderProcessId));
            }

            using (DatabaseContextFactory.Create(EventStoreProjectionConnectionStringName))
            {
                DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from ProjectionPosition"));
            }
        }

        public IDatabaseContextCache DatabaseContextCache { get; private set; }

        public DatabaseGateway DatabaseGateway { get; private set; }
        public IDatabaseContextFactory DatabaseContextFactory { get; private set; }
        public IConnectionConfigurationProvider ConnectionConfigurationProvider { get; private set; }

        public string EventStoreConnectionStringName = "EventStore";
        public string EventStoreProjectionConnectionStringName = "EventStoreProjection";

        protected void Boostrap(IComponentRegistry registry)
        {
#if (NETCOREAPP2_0 || NETSTANDARD2_0)
            registry.Register<IDbProviderFactories, DbProviderFactories>();

            var connectionConfigurationProvider = new Mock<IConnectionConfigurationProvider>();

            connectionConfigurationProvider.Setup(m => m.Get(It.IsAny<string>())).Returns(
                (string name) =>
                    name.Equals("EventStoreProjection")
                        ? new ConnectionConfiguration(
                            "EventStoreProjection",
                            "System.Data.SqlClient",
                            "Data Source=.\\sqlexpress;Initial Catalog=ShuttleProjection;Integrated Security=SSPI;")
                        : new ConnectionConfiguration(
                            name,
                            "System.Data.SqlClient",
                            "Data Source=.\\sqlexpress;Initial Catalog=Shuttle;Integrated Security=SSPI;"));

            registry.AttemptRegisterInstance(connectionConfigurationProvider.Object);

            registry.RegisterInstance<IProjectionConfiguration>(new ProjectionConfiguration
            {
                EventProjectionConnectionString =
                    connectionConfigurationProvider.Object.Get("EventStoreProjection").ConnectionString,
                EventProjectionProviderName =
                    connectionConfigurationProvider.Object.Get("EventStoreProjection").ProviderName,
                EventProjectionPrefetchCount = 1,
                EventStoreConnectionString = connectionConfigurationProvider.Object.Get("Shuttle").ConnectionString,
                EventStoreProviderName = connectionConfigurationProvider.Object.Get("Shuttle").ProviderName
            });
#else
            registry.AttemptRegister<IConnectionConfigurationProvider, ConnectionConfigurationProvider>();
#endif
        }
    }
}