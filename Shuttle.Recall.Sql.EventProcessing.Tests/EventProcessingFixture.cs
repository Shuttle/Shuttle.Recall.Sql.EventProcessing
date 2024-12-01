using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Recall.Sql.Storage;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.Sql.EventProcessing.Tests;

public class EventProcessingFixture : RecallFixture
{
    [Test]
    public async Task Should_be_able_to_process_events_async()
    {
        var services = SqlConfiguration.GetServiceCollection();

        var serviceProvider = services.BuildServiceProvider();
        var databaseContextFactory = serviceProvider.GetRequiredService<IDatabaseContextFactory>();
        var options = serviceProvider.GetRequiredService<IOptions<SqlStorageOptions>>().Value;

        await using (var databaseContext = databaseContextFactory.Create("StorageConnection"))
        {
            await databaseContext.ExecuteAsync(new Query($"DELETE FROM [{options.Schema}].[PrimitiveEvent] WHERE Id IN ('{OrderId}', '{OrderProcessId}')"));
            await databaseContext.ExecuteAsync(new Query($"DELETE FROM [{options.Schema}].[PrimitiveEventJournal] WHERE Id IN ('{OrderId}', '{OrderProcessId}')"));
        }

        await ExerciseEventProcessingAsync(services, (EventStoreBuilder builder) =>
        {
            builder.Options.ProjectionThreadCount = 1;
        }, handlerTimeoutSeconds: 300);
    }
}