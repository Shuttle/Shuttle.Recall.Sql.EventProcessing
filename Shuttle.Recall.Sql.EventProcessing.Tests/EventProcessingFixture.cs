using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Recall.Tests;

namespace Shuttle.Recall.Sql.EventProcessing.Tests;

public class EventProcessingFixture : RecallFixture
{
    [Test]
    public void ExerciseEventProcessing()
    {
        var services = SqlConfiguration.GetServiceCollection();

        var serviceProvider = services.BuildServiceProvider();

        var databaseContextFactory = serviceProvider.GetRequiredService<IDatabaseContextFactory>();

        using (databaseContextFactory.Create("Shuttle"))
        {
            ExerciseStorageRemoval(services);
            ExerciseStorage(services);
        }

        ExerciseEventProcessing(services, 300);

        using (databaseContextFactory.Create("Shuttle"))
        {
            ExerciseStorageRemoval(services);
        }
    }
}