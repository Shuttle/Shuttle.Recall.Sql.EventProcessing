using System;
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
    [Test]
    public async Task Should_be_able_to_process_events_async()
    {
        var services = SqlConfiguration.GetServiceCollection();

        IDatabaseContextFactory? databaseContextFactory = null;
        SqlStorageOptions? sqlStorageOptions = null;

        var fixtureConfiguration = new FixtureConfiguration(services)
            .WithServiceProviderCallback(serviceProvider =>
            {
                databaseContextFactory = serviceProvider.GetRequiredService<IDatabaseContextFactory>();
                sqlStorageOptions = serviceProvider.GetRequiredService<IOptions<SqlStorageOptions>>().Value;
            })
            .WithRemoveIdsCallback(async ids =>
            {
                await using (var databaseContext = databaseContextFactory!.Create("StorageConnection"))
                {
                    await databaseContext.ExecuteAsync(new Query($"DELETE FROM [{sqlStorageOptions!.Schema}].[PrimitiveEvent] WHERE Id IN ({string.Join(',', ids.Select(id => $"'{id}'"))})"));
                }
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

        IDatabaseContextFactory? databaseContextFactory = null;
        SqlStorageOptions? sqlStorageOptions = null;

        var fixtureConfiguration = new FixtureConfiguration(services)
            .WithServiceProviderCallback(serviceProvider =>
            {
                databaseContextFactory = serviceProvider.GetRequiredService<IDatabaseContextFactory>();
                sqlStorageOptions = serviceProvider.GetRequiredService<IOptions<SqlStorageOptions>>().Value;
            })
            .WithRemoveIdsCallback(async ids =>
            {
                await using (var databaseContext = databaseContextFactory!.Create("StorageConnection"))
                {
                    await databaseContext.ExecuteAsync(new Query($"DELETE FROM [{sqlStorageOptions!.Schema}].[PrimitiveEvent] WHERE Id IN ({string.Join(',', ids.Select(id => $"'{id}'"))})"));
                }
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

        IDatabaseContextFactory? databaseContextFactory = null;
        SqlStorageOptions? sqlStorageOptions = null;

        var fixtureConfiguration = new FixtureConfiguration(services)
            .WithServiceProviderCallback(serviceProvider =>
            {
                databaseContextFactory = serviceProvider.GetRequiredService<IDatabaseContextFactory>();
                sqlStorageOptions = serviceProvider.GetRequiredService<IOptions<SqlStorageOptions>>().Value;
            })
            .WithRemoveIdsCallback(async ids =>
            {
                await using (var databaseContext = databaseContextFactory!.Create("StorageConnection"))
                {
                    await databaseContext.ExecuteAsync(new Query($"DELETE FROM [{sqlStorageOptions!.Schema}].[PrimitiveEvent] WHERE Id IN ({string.Join(',', ids.Select(id => $"'{id}'"))})"));
                }
            })
            .WithEventStreamTaskCallback(async task =>
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