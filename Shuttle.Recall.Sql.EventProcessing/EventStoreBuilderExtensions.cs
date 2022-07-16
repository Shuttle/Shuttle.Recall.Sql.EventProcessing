using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Core.Pipelines;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public static class EventStoreBuilderExtensions
    {
        public static EventStoreBuilder UseSqlEventProcessing(this EventStoreBuilder eventStoreBuilder, EventProcessingOptions eventProcessingOptions)
        {
            Guard.AgainstNull(eventStoreBuilder, nameof(eventStoreBuilder));
            Guard.AgainstNull(eventProcessingOptions, nameof(eventProcessingOptions));

            eventStoreBuilder.Services.TryAddSingleton<IScriptProvider, ScriptProvider>();
            eventStoreBuilder.Services.TryAddSingleton<IValidateOptions<EventProcessingOptions>, EventProcessingValidator>();
            eventStoreBuilder.Services.TryAddSingleton<IProjectionRepository, ProjectionRepository>();
            eventStoreBuilder.Services.TryAddSingleton<IProjectionQueryFactory, ProjectionQueryFactory>();

            eventStoreBuilder.Services.TryAddSingleton<EventProcessingObserver, EventProcessingObserver>();
            eventStoreBuilder.Services.TryAddSingleton<EventProcessingModule, EventProcessingModule>();

            eventStoreBuilder.Services.AddPipelineModule<EventProcessingModule>();

            eventStoreBuilder.Services.AddOptions<EventProcessingOptions>().Configure(options =>
            {
                options.EventProjectionConnectionStringName =
                    eventProcessingOptions.EventProjectionConnectionStringName;
                options.EventStoreConnectionStringName =
                    eventProcessingOptions.EventStoreConnectionStringName;
            });

            return eventStoreBuilder;
        }
    }
}