using System;
using System.Data.Common;
using System.Data.SqlClient;
using System.Transactions;
using Moq;
using NUnit.Framework;
using Shuttle.Core.Container;
using Shuttle.Core.Data;
using Shuttle.Core.Transactions;
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

            DatabaseGateway = new DatabaseGateway();
            DatabaseContextCache = new ThreadStaticDatabaseContextCache();

            var connectionConfigurationProvider = new Mock<IConnectionConfigurationProvider>();

            connectionConfigurationProvider.Setup(m => m.Get(It.IsAny<string>())).Returns(
                (string name) =>
                    new ConnectionConfiguration(
                        name,
                        "System.Data.SqlClient",
                        name.Equals(EventStoreProjectionConnectionStringName)
                            ? "server=.;Initial Catalog=ShuttleProjection;user id=sa;password=Pass!000"
                            : "server=.;Initial Catalog=Shuttle;user id=sa;password=Pass!000"));

            ConnectionConfigurationProvider = connectionConfigurationProvider.Object;

            DatabaseContextFactory = new DatabaseContextFactory(
                ConnectionConfigurationProvider,
                new DbConnectionFactory(),
                new DbCommandFactory(),
                new ThreadStaticDatabaseContextCache());

            ClearDataStore();
        }

        [TearDown]
        protected void ClearDataStore()
        {
            using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
            {
                DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from EventStore where Id = @Id")
                    .AddParameterValue(Columns.Id, RecallFixture.OrderId));
                DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from EventStore where Id = @Id")
                    .AddParameterValue(Columns.Id, RecallFixture.OrderProcessId));
                DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from SnapshotStore where Id = @Id")
                    .AddParameterValue(Columns.Id, RecallFixture.OrderId));
                DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from SnapshotStore where Id = @Id")
                    .AddParameterValue(Columns.Id, RecallFixture.OrderProcessId));
            }

            using (DatabaseContextFactory.Create(EventStoreProjectionConnectionStringName))
            {
                DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from Projection"));
            }
        }

        public IDatabaseContextCache DatabaseContextCache { get; private set; }
        public DatabaseGateway DatabaseGateway { get; private set; }
        public IDatabaseContextFactory DatabaseContextFactory { get; private set; }
        public IConnectionConfigurationProvider ConnectionConfigurationProvider { get; private set; }

        public string EventStoreConnectionStringName = "EventStore";
        public string EventStoreProjectionConnectionStringName = "EventStoreProjection";

        protected void Bootstrap(IComponentRegistry registry)
        {
            registry.RegisterInstance<ITransactionScopeFactory>(new DefaultTransactionScopeFactory(false, IsolationLevel.Unspecified, TimeSpan.Zero));

            registry.AttemptRegisterInstance(ConnectionConfigurationProvider);

            registry.RegisterInstance<IProjectionConfiguration>(new ProjectionConfiguration
            {
                EventProjectionConnectionString = ConnectionConfigurationProvider.Get(EventStoreProjectionConnectionStringName).ConnectionString,
                EventProjectionProviderName = ConnectionConfigurationProvider.Get(EventStoreProjectionConnectionStringName).ProviderName,
                EventStoreConnectionString = ConnectionConfigurationProvider.Get(EventStoreConnectionStringName).ConnectionString,
                EventStoreProviderName = ConnectionConfigurationProvider.Get(EventStoreConnectionStringName).ProviderName
            });
        }
    }
}