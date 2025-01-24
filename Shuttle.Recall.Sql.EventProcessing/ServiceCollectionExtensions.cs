using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Recall.Sql.EventProcessing;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSqlEventProcessing(this IServiceCollection services, Action<SqlEventProcessingBuilder>? builder = null)
    {
        var eventProcessingBuilder = new SqlEventProcessingBuilder(Guard.AgainstNull(services));

        builder?.Invoke(eventProcessingBuilder);

        services.AddSingleton<IValidateOptions<SqlEventProcessingOptions>, SqlEventProcessingOptionsValidator>();
        services.AddSingleton<IProjectionQuery, ProjectionQuery>();
        services.AddSingleton<IProjectionRepository, ProjectionRepository>();
        services.AddSingleton<IProjectionService, ProjectionService>();
        services.AddSingleton<EventProcessingStartupObserver>();

        services.AddSingleton<DatabaseContextObserver, DatabaseContextObserver>();

        services.AddOptions<SqlEventProcessingOptions>().Configure(options =>
        {
            options.ConnectionStringName = eventProcessingBuilder.Options.ConnectionStringName;
            options.Schema = eventProcessingBuilder.Options.Schema;
            options.ConfigureDatabase = eventProcessingBuilder.Options.ConfigureDatabase;
            options.RegisterDatabaseContextObserver = eventProcessingBuilder.Options.RegisterDatabaseContextObserver;
            options.ProjectionJournalSize = eventProcessingBuilder.Options.ProjectionJournalSize;
        });

        services.AddHostedService<EventProcessingHostedService>();

        return services;
    }
}