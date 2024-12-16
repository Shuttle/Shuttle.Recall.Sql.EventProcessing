using System;
using System.Collections.Generic;
using System.Linq;
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
    private static async Task RemoveIds(IServiceProvider serviceProvider, IEnumerable<Guid> ids)
    {
        var sqlStorageOptions = serviceProvider.GetRequiredService<IOptions<SqlStorageOptions>>().Value;

        await using (var databaseContext = serviceProvider.GetRequiredService<IDatabaseContextFactory>().Create("StorageConnection"))
        {
            await databaseContext.ExecuteAsync(new Query($@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{sqlStorageOptions.Schema}].[PrimitiveEvent]') AND type in (N'U'))
BEGIN
    DELETE FROM [{sqlStorageOptions.Schema}].[PrimitiveEvent] WHERE Id IN ({string.Join(',', ids.Select(id => $"'{id}'"))})
END
"
            ));
        }
    }

    [Test]
    public async Task Should_be_able_to_process_events_async()
    {
        var services = SqlConfiguration.GetServiceCollection();

        var fixtureConfiguration = new FixtureConfiguration(services)
            .WithRemoveIdsCallback(async (serviceProvider, ids) =>
            {
                await RemoveIds(serviceProvider, ids);
            })
            .WithEventStoreBuilderCallback(builder =>
            {
                builder.Options.ProjectionThreadCount = 1;
            })
            .WithHandlerTimeout(TimeSpan.FromMinutes(5));

        await ExerciseEventProcessingAsync(fixtureConfiguration);
    }

    [Test]
    public async Task Should_be_able_to_process_events_with_failure_async()
    {
        var services = SqlConfiguration.GetServiceCollection();

        var fixtureConfiguration = new FixtureConfiguration(services)
            .WithRemoveIdsCallback(async (serviceProvider, ids) =>
            {
                await RemoveIds(serviceProvider, ids);
            })
            .WithEventStoreBuilderCallback(builder =>
            {
                builder.Options.ProjectionThreadCount = 1;
            });

        await ExerciseEventProcessingWithFailureAsync(fixtureConfiguration);
    }

    [Test]
    public async Task Should_be_able_to_process_events_with_delay_async()
    {
        var services = SqlConfiguration.GetServiceCollection();

        var fixtureConfiguration = new FixtureConfiguration(services)
            .WithRemoveIdsCallback(async (serviceProvider, ids) =>
            {
                await RemoveIds(serviceProvider, ids);
            })
            .WithEventStreamTaskCallback(async (_, task) =>
            {
                using (new DatabaseContextScope())
                {
                    await task();
                }
            })
            .WithEventStoreBuilderCallback(builder =>
            {
                builder.Options.ProjectionThreadCount = 1;
            })
            .WithHandlerTimeout(TimeSpan.FromMinutes(5));

        await ExerciseEventProcessingWithDelayAsync(fixtureConfiguration);
    }
}