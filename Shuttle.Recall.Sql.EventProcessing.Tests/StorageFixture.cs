using Castle.Windsor;
using NUnit.Framework;
using Shuttle.Core.Castle;
using Shuttle.Core.Data;
using Shuttle.Recall.Tests;
using Shuttle.Core.Container;

namespace Shuttle.Recall.Sql.EventProcessing.Tests
{
    public class StorageFixture : Fixture
    {
        [Test]
        public void ExerciseEventProcessing()
        {
            var container = new WindsorComponentContainer(new WindsorContainer());

            Boostrap(container);

            EventStore.Register(container);

            using (container.Resolve<IDatabaseContextFactory>().Create(EventStoreConnectionStringName))
            {
                RecallFixture.ExerciseEventProcessing(EventStore.Create(container), EventProcessor.Create(container), 300);
            }
        }
    }
}