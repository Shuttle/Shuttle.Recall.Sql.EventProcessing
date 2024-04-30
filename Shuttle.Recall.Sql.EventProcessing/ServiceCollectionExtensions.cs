using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlEventProcessing(this IServiceCollection services, Action<SqlEventProcessingBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var eventProcessingBuilder = new SqlEventProcessingBuilder(services);

            builder?.Invoke(eventProcessingBuilder);

            services.TryAddSingleton<IScriptProvider, ScriptProvider>();
            services.TryAddSingleton<IValidateOptions<SqlEventProcessingOptions>, SqlEventProcessingOptionsValidator>();
            services.AddSingleton<IProjectionRepository, ProjectionRepository>();
            services.AddSingleton<IProjectionQueryFactory, ProjectionQueryFactory>();

            services.TryAddSingleton<EventProcessingObserver, EventProcessingObserver>();
            services.TryAddSingleton<AddProjectionObserver, AddProjectionObserver>();

            services.AddOptions<SqlEventProcessingOptions>().Configure(options =>
            {
                options.ConnectionStringName = eventProcessingBuilder.Options.ConnectionStringName;
                options.ManageEventStoreConnections = eventProcessingBuilder.Options.ManageEventStoreConnections;
            });

            services.AddHostedService<EventProcessingHostedService>();

            return services;
        }
    }
}