using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Shuttle.Core.Data;
using Shuttle.Core.Data.Logging;
using Shuttle.Recall.Logging;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Recall.Sql.EventProcessing.Tests;

[SetUpFixture]
public class SqlConfiguration
{
    public static IServiceCollection GetServiceCollection()
    {
        var services = new ServiceCollection();

        services
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddDataAccess(builder =>
            {
                builder.AddConnectionString("Shuttle", "Microsoft.Data.SqlClient", "server=.;database=Shuttle;user id=sa;password=Pass!000;TrustServerCertificate=true");
                builder.AddConnectionString("ShuttleProjection", "Microsoft.Data.SqlClient", "server=.;database=ShuttleProjection;user id=sa;password=Pass!000;TrustServerCertificate=true");
                builder.Options.DatabaseContextFactory.DefaultConnectionStringName = "Shuttle";
            })
            .AddDataAccessLogging(builder =>
            {
                builder.Options.DatabaseContext = false;
                builder.Options.DbCommandFactory = true;
            })
            .AddSqlEventStorage(builder =>
            {
                builder.Options.ConnectionStringName = "Shuttle";
            })
            .AddSqlEventProcessing(builder =>
            {
                builder.Options.ConnectionStringName = "ShuttleProjection";
            })
            .AddEventStore(builder =>
            {
                builder.Options.ProjectionThreadCount = 1;
            })
            .AddRecallLogging();

        return services;
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
    }
}