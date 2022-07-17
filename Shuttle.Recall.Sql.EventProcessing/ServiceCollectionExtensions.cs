using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection UseSqlEventProcessing(this IServiceCollection services, EventProcessingOptions eventProcessingOptions)
        {
            Guard.AgainstNull(services, nameof(services));
            Guard.AgainstNull(eventProcessingOptions, nameof(eventProcessingOptions));

            services.TryAddSingleton<IScriptProvider, ScriptProvider>();
            services.TryAddSingleton<IValidateOptions<EventProcessingOptions>, EventProcessingValidator>();
            services.TryAddSingleton<IProjectionRepository, ProjectionRepository>();
            services.TryAddSingleton<IProjectionQueryFactory, ProjectionQueryFactory>();

            services.TryAddSingleton<EventProcessingObserver, EventProcessingObserver>();
            services.TryAddSingleton<EventProcessingModule, EventProcessingModule>();

            services.AddPipelineModule<EventProcessingModule>();

            services.AddOptions<EventProcessingOptions>().Configure(options =>
            {
                options.EventProjectionConnectionStringName =
                    eventProcessingOptions.EventProjectionConnectionStringName;
                options.EventStoreConnectionStringName =
                    eventProcessingOptions.EventStoreConnectionStringName;
            });

            return services;
        }
    }
}