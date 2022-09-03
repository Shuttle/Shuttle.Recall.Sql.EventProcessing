using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Core.Transactions;
using Shuttle.Recall.Sql.Storage;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.Sql.EventProcessing.Tests
{
    public class EventProcessingFixture : Fixture
    {
        [Test]
        public void ExerciseEventProcessing()
        {
            var services = new ServiceCollection();

            services.AddDataAccess(builder =>
            {
                builder.AddConnectionString(EventStoreConnectionStringName, "System.Data.SqlClient",
                    "server=.;Initial Catalog=Shuttle;user id=sa;password=Pass!000");
                builder.AddConnectionString(EventProjectionConnectionStringName, "System.Data.SqlClient",
                    "server=.;Initial Catalog=ShuttleProjection;user id=sa;password=Pass!000");
            });

            services.AddTransactionScope(builder =>
            {
                builder.Options.Enabled = false;
            });

            services.AddEventStore();

            services.AddSqlEventStorage();
            services.AddSqlEventProcessing(builder =>
            {
                builder.Options.EventProjectionConnectionStringName = EventProjectionConnectionStringName;
                builder.Options.EventStoreConnectionStringName = EventStoreConnectionStringName;
            });

            var serviceProvider = services.BuildServiceProvider();

            var eventStore = serviceProvider.GetRequiredService<IEventStore>();

            using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
            {
                RecallFixture.ExerciseStorage(eventStore);
            }

            using (DatabaseContextFactory.Create(EventProjectionConnectionStringName))
            {
                RecallFixture.ExerciseEventProcessing(serviceProvider.GetRequiredService<IEventProcessor>(), 300);
            }

            using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
            {
                RecallFixture.ExerciseStorageRemoval(eventStore);
            }
        }
    }
}