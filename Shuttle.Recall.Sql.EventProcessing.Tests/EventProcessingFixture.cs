using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Data;
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

        await using (var databaseContext = databaseContextFactory.Create())
        {
            await databaseContext.ExecuteAsync(new Query("delete from EventStore where Id = @Id").AddParameter(Columns.Id, OrderId));
            await databaseContext.ExecuteAsync(new Query("delete from EventStore where Id = @Id").AddParameter(Columns.Id, OrderProcessId));
        }

        await ExerciseEventProcessingAsync(services, (EventStoreBuilder builder) =>
        {
            builder.Options.ProjectionThreadCount = 1;
        }, handlerTimeoutSeconds: 300);
    }
}