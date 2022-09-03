using System.Data.Common;
using System.Data.SqlClient;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.Sql.EventProcessing.Tests
{
    [TestFixture]
    public class Fixture
    {
        [SetUp]
        public void TestSetUp()
        {
            DbProviderFactories.RegisterFactory("System.Data.SqlClient", SqlClientFactory.Instance);

            DatabaseContextCache = new ThreadStaticDatabaseContextCache();
            DatabaseGateway = new DatabaseGateway(DatabaseContextCache);

            var connectionStringOptions = new Mock<IOptionsMonitor<ConnectionStringOptions>>();

            connectionStringOptions.Setup(m => m.Get(It.IsAny<string>())).Returns(
                (string name) => new ConnectionStringOptions
                {
                    Name = name,
                    ProviderName = "System.Data.SqlClient",
                    ConnectionString = name.Equals(EventProjectionConnectionStringName)
                        ? "server=.;Initial Catalog=ShuttleProjection;user id=sa;password=Pass!000"
                        : "server=.;Initial Catalog=Shuttle;user id=sa;password=Pass!000"
                });

            ConnectionStringOptions = connectionStringOptions.Object;

            DatabaseContextFactory = new DatabaseContextFactory(
                ConnectionStringOptions,
                Options.Create(new DataAccessOptions
                {
                    DatabaseContextFactory = new DatabaseContextFactoryOptions
                    {
                        DefaultConnectionStringName = "Shuttle"
                    }
                }),
                new DbConnectionFactory(),
                new DbCommandFactory(Options.Create(new DataAccessOptions())),
                new ThreadStaticDatabaseContextCache());

            ClearDataStore();
        }

        [TearDown]
        protected void ClearDataStore()
        {
            using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
            {
                DatabaseGateway.Execute(RawQuery.Create("delete from EventStore where Id = @Id")
                    .AddParameterValue(Columns.Id, RecallFixture.OrderId));
                DatabaseGateway.Execute(RawQuery.Create("delete from EventStore where Id = @Id")
                    .AddParameterValue(Columns.Id, RecallFixture.OrderProcessId));
                DatabaseGateway.Execute(RawQuery.Create("delete from SnapshotStore where Id = @Id")
                    .AddParameterValue(Columns.Id, RecallFixture.OrderId));
                DatabaseGateway.Execute(RawQuery.Create("delete from SnapshotStore where Id = @Id")
                    .AddParameterValue(Columns.Id, RecallFixture.OrderProcessId));
            }

            using (DatabaseContextFactory.Create(EventProjectionConnectionStringName))
            {
                DatabaseGateway.Execute(RawQuery.Create("delete from Projection where [Name] = 'recall-fixture'"));
            }
        }

        public IDatabaseContextCache DatabaseContextCache { get; private set; }
        public DatabaseGateway DatabaseGateway { get; private set; }
        public IDatabaseContextFactory DatabaseContextFactory { get; private set; }
        public IOptionsMonitor<ConnectionStringOptions> ConnectionStringOptions { get; private set; }

        public string EventStoreConnectionStringName = "EventStore";
        public string EventProjectionConnectionStringName = "EventStoreProjection";
    }
}