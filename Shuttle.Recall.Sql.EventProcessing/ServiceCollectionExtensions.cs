using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlEventProcessing(this IServiceCollection services, Action<SqlEventProcessingBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var eventProcessingBuilder = new SqlEventProcessingBuilder(services);

            builder?.Invoke(eventProcessingBuilder);

            services.AddSingleton<IScriptProvider, ScriptProvider>();
            services.AddSingleton<IValidateOptions<SqlEventProcessingOptions>, SqlEventProcessingOptionsValidator>();
            services.AddSingleton<IProjectionRepository, ProjectionRepository>();
            services.AddSingleton<IProjectionQueryFactory, ProjectionQueryFactory>();

            services.AddSingleton<EventProcessingObserver, EventProcessingObserver>();
            services.AddSingleton<AddProjectionObserver, AddProjectionObserver>();

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