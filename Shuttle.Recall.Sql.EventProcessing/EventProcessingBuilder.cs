using System;
using Microsoft.Extensions.DependencyInjection;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.Sql.EventProcessing;

public class SqlEventProcessingBuilder
{
    private SqlEventProcessingOptions _sqlEventProcessingOptions = new();

    public SqlEventProcessingBuilder(IServiceCollection services)
    {
        Services = Guard.AgainstNull(services);
    }

    public SqlEventProcessingOptions Options
    {
        get => _sqlEventProcessingOptions;
        set => _sqlEventProcessingOptions = Guard.AgainstNull(value);
    }

    public IServiceCollection Services { get; }
}