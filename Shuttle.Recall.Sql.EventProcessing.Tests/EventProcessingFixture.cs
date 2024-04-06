using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.Sql.EventProcessing.Tests;

public class EventProcessingFixture : RecallFixture
{
    [Test]
    public void Should_be_able_to_process_events()
    {
        Should_be_able_to_process_events_async(true).GetAwaiter().GetResult();
    }

    [Test]
    public async Task Should_be_able_to_process_events_async()
    {
        await Should_be_able_to_process_events_async(false);
    }

    private async Task Should_be_able_to_process_events_async(bool sync)
    {
        var services = SqlConfiguration.GetServiceCollection(sync);

        var serviceProvider = services.BuildServiceProvider();
        var databaseGateway = serviceProvider.GetRequiredService<IDatabaseGateway>();
        var databaseContextFactory = serviceProvider.GetRequiredService<IDatabaseContextFactory>();

        using (databaseContextFactory.Create())
        {
            await databaseGateway.ExecuteAsync(new Query("delete from EventStore where Id = @Id").AddParameter(Columns.Id, OrderId));
            await databaseGateway.ExecuteAsync(new Query("delete from EventStore where Id = @Id").AddParameter(Columns.Id, OrderProcessId));
            await databaseGateway.ExecuteAsync(new Query("delete from SnapshotStore where Id = @Id").AddParameter(Columns.Id, OrderId));
            await databaseGateway.ExecuteAsync(new Query("delete from SnapshotStore where Id = @Id").AddParameter(Columns.Id, OrderProcessId));
        }

        if (sync)
        {
            ExerciseStorage(services);
            ExerciseEventProcessing(services, handlerTimeoutSeconds: 300);
            ExerciseStorageRemoval(services);
        }
        else
        {
            await ExerciseStorageAsync(services);
            await ExerciseEventProcessingAsync(services, handlerTimeoutSeconds: 300);
            await ExerciseStorageRemovalAsync(services);
        }
    }
}