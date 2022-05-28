using Ninject;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Core.Ninject;
using Shuttle.Recall.Sql.Storage;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.Sql.EventProcessing.Tests
{
    public class EventProcessingFixture : Fixture
    {
        [Test]
        public void ExerciseEventProcessing()
        {
            var container = new NinjectComponentContainer(new StandardKernel());

            Bootstrap(container);

            container.RegisterDataAccess();
            container.RegisterEventStore();
            container.RegisterEventStoreStorage();
            container.RegisterEventProcessing();

            container.Resolve<EventProcessingModule>();

            var eventStore = container.Resolve<IEventStore>();

            using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
            {
                RecallFixture.ExerciseStorage(eventStore);
            }

            using (DatabaseContextFactory.Create(EventStoreProjectionConnectionStringName))
            {
                RecallFixture.ExerciseEventProcessing(container.Resolve<IEventProcessor>(), 300);
            }

            using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
            {
                RecallFixture.ExerciseStorageRemoval(eventStore);
            }
        }
    }
}