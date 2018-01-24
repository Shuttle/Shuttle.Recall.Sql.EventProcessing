using Moq;
using NUnit.Framework;
using Shuttle.Core.Data;
#if (NETCOREAPP2_0 || NETSTANDARD2_0)
using Shuttle.Core.Data.SqlClient;
#endif

namespace Shuttle.Recall.Sql.EventProcessing.Tests
{
	[TestFixture]
	public class Fixture
	{
	    public IDatabaseContextCache DatabaseContextCache { get; private set; }

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
		    var connectionConfigurationProvider = new Mock<IConnectionConfigurationProvider>();

		    connectionConfigurationProvider.Setup(m => m.Get(It.IsAny<string>())).Returns(
		        new ConnectionConfiguration(
		            "Shuttle",
		            "System.Data.SqlClient",
		            "Data Source=.\\sqlexpress;Initial Catalog=shuttle;Integrated Security=SSPI;"));

		    DatabaseContextFactory = new DatabaseContextFactory(
		        connectionConfigurationProvider.Object,
		        new DbConnectionFactory(new DbProviderFactories()),
		        new DbCommandFactory(),
		        new ThreadStaticDatabaseContextCache());
#endif

            ClearDataStore();
		}

		public DatabaseGateway DatabaseGateway { get; private set; }
		public IDatabaseContextFactory DatabaseContextFactory { get; private set; }

		public string EventStoreConnectionStringName = "EventStore";
		public string EventStoreProjectionConnectionStringName = "EventStoreProjection";

		[TearDown]
		protected void ClearDataStore()
		{
            using (DatabaseContextFactory.Create(EventStoreProjectionConnectionStringName))
			{
                DatabaseGateway.ExecuteUsing(RawQuery.Create("delete from ProjectionPosition"));
            }
        }
	}
}