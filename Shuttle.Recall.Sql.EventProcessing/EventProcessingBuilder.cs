using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public class SqlEventProcessingBuilder
    {
        private SqlEventProcessingOptions _sqlEventProcessingOptions = new SqlEventProcessingOptions();

        public IServiceCollection Services { get; }

        public SqlEventProcessingBuilder(IServiceCollection services)
        {
            Services = Guard.AgainstNull(services, nameof(services));
        }

        public SqlEventProcessingOptions Options
        {
            get => _sqlEventProcessingOptions;
            set => _sqlEventProcessingOptions = value ?? throw new ArgumentNullException(nameof(value));
        }
    }
}