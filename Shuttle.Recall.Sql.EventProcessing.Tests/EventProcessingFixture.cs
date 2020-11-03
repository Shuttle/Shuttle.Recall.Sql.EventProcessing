using Castle.Windsor;
using NUnit.Framework;
using Shuttle.Core.Castle;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.Sql.EventProcessing.Tests
{
    public class EventProcessingFixture : Fixture
    {
        [Test]
        public void ExerciseEventProcessing()
        {
            var container = new WindsorComponentContainer(new WindsorContainer());

            Bootstrap(container);

            EventStore.Register(container);

            var eventStore = EventStore.Create(container);

            using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
            {
                RecallFixture.ExerciseStorage(eventStore);
            }

            using (DatabaseContextFactory.Create(EventStoreProjectionConnectionStringName))
            {
                
                RecallFixture.ExerciseEventProcessing(EventProcessor.Create(container), 300);
            }

            using (DatabaseContextFactory.Create(EventStoreConnectionStringName))
            {
                RecallFixture.ExerciseStorageRemoval(eventStore);
            }
        }
    }
}