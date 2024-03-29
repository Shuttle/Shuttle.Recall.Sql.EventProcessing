﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlEventProcessing(this IServiceCollection services, Action<EventProcessingBuilder> builder = null)
        {
            Guard.AgainstNull(services, nameof(services));

            var eventProcessingBuilder = new EventProcessingBuilder(services);

            builder?.Invoke(eventProcessingBuilder);

            services.TryAddSingleton<IScriptProvider, ScriptProvider>();
            services.TryAddSingleton<IValidateOptions<EventProcessingOptions>, EventProcessingValidator>();
            services.AddSingleton<IProjectionRepository, ProjectionRepository>();
            services.AddSingleton<IProjectionQueryFactory, ProjectionQueryFactory>();

            services.TryAddSingleton<EventProcessingObserver, EventProcessingObserver>();
            services.TryAddSingleton<EventProcessingModule, EventProcessingModule>();

            services.AddPipelineModule<EventProcessingModule>();

            services.AddOptions<EventProcessingOptions>().Configure(options =>
            {
                options.EventProjectionConnectionStringName =
                    eventProcessingBuilder.Options.EventProjectionConnectionStringName;
                options.EventStoreConnectionStringName =
                    eventProcessingBuilder.Options.EventStoreConnectionStringName;
            });

            return services;
        }
    }
}