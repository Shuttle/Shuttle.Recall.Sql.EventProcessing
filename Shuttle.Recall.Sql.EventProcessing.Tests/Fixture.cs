using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Recall.Tests;

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
            DatabaseContextFactory = new DatabaseContextFactory(new DbConnectionFactory(), new DbCommandFactory(), DatabaseContextCache);

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