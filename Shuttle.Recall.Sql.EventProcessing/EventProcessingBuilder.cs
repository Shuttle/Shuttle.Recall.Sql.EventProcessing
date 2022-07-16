using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class EventProcessingBuilder
    {
        private EventProcessingOptions _eventProcessingOptions = new EventProcessingOptions();

        public IServiceCollection Services { get; }

        public EventProcessingBuilder(IServiceCollection services)
        {
            Guard.AgainstNull(services, nameof(services));

            Services = services;
        }

        public EventProcessingOptions Options
        {
            get => _eventProcessingOptions;
            set => _eventProcessingOptions = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}