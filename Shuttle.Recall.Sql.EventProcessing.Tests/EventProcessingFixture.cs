using System;
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

        var fixtureConfiguration = new FixtureConfiguration(services)
            .WithStarting(StartingAsync)
            .WithAddEventStore(builder =>
            {
                builder.Options.ProjectionThreadCount = 1;
            })
            .WithHandlerTimeout(TimeSpan.FromMinutes(5));

        await ExerciseEventProcessingAsync(fixtureConfiguration);
    }

    [Test]
    public async Task Should_be_able_to_process_events_with_delay_async()
    {
        var services = SqlConfiguration.GetServiceCollection();

        var fixtureConfiguration = new FixtureConfiguration(services)
            .WithStarting(StartingAsync)
            .WithEventStreamTask(async (_, task) =>
            {
                using (new DatabaseContextScope())
                {
                    await task();
                }
            })
            .WithAddEventStore(builder =>
            {
                builder.Options.ProjectionThreadCount = 1;
            })
            .WithHandlerTimeout(TimeSpan.FromMinutes(5));

        await ExerciseEventProcessingWithDelayAsync(fixtureConfiguration);
    }

    [Test]
    public async Task Should_be_able_to_process_events_with_failure_async()
    {
        var services = SqlConfiguration.GetServiceCollection();

        var fixtureConfiguration = new FixtureConfiguration(services)
            .WithStarting(StartingAsync)
            .WithAddEventStore(builder =>
            {
                builder.Options.ProjectionThreadCount = 1;
            });

        await ExerciseEventProcessingWithFailureAsync(fixtureConfiguration);
    }

    [Test]
    public async Task Should_be_able_to_process_volume_events_async()
    {
        var services = SqlConfiguration.GetServiceCollection();

        var fixtureConfiguration = new FixtureConfiguration(services)
            .WithStarting(StartingAsync)
            .WithEventStreamTask(async (_, task) =>
            {
                using (new DatabaseContextScope())
                {
                    await task();
                }
            })
            .WithAddEventStore(builder =>
            {
                builder.Options.ProjectionThreadCount = 25;
            })
            .WithHandlerTimeout(TimeSpan.FromMinutes(5));

        await ExerciseEventProcessingVolumeAsync(fixtureConfiguration);
    }

    private static async Task StartingAsync(IServiceProvider serviceProvider)
    {
        var sqlStorageOptions = serviceProvider.GetRequiredService<IOptions<SqlStorageOptions>>().Value;
        var sqlEventProcessingOptions = serviceProvider.GetRequiredService<IOptions<SqlEventProcessingOptions>>().Value;

        await using (var databaseContext = serviceProvider.GetRequiredService<IDatabaseContextFactory>().Create("StorageConnection"))
        {
            await databaseContext.ExecuteAsync(new Query($@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{sqlStorageOptions.Schema}].[PrimitiveEvent]') AND type in (N'U'))
BEGIN
    DELETE FROM [{sqlStorageOptions.Schema}].[PrimitiveEvent]
    FROM
        [{sqlStorageOptions.Schema}].[PrimitiveEvent] pe
    INNER JOIN 
        [{sqlStorageOptions.Schema}].[EventType] et ON pe.EventTypeId = et.Id
    WHERE
        et.[TypeName] LIKE 'Shuttle.Recall.Tests%'
END
"
            ));
        }

        await using (var databaseContext = serviceProvider.GetRequiredService<IDatabaseContextFactory>().Create("EventProcessingConnection"))
        {
            await databaseContext.ExecuteAsync(new Query($@"
IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{sqlEventProcessingOptions.Schema}].[Projection]') AND type in (N'U'))
BEGIN
    DELETE FROM [{sqlEventProcessingOptions.Schema}].[Projection] WHERE [Name] like 'recall-fixture%'
END

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[{sqlEventProcessingOptions.Schema}].[ProjectionJournal]') AND type in (N'U'))
BEGIN
    DELETE FROM [{sqlEventProcessingOptions.Schema}].[ProjectionJournal] WHERE [Name] like 'recall-fixture%'
END
"
            ));
        }
    }
}