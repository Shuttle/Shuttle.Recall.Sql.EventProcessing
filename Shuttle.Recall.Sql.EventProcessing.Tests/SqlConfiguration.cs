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
                builder.AddConnectionString("StorageConnection", "Microsoft.Data.SqlClient", "Server=.;Database=RecallFixtureStorage;User Id=sa;Password=Pass!000;TrustServerCertificate=true");
                builder.AddConnectionString("EventProcessingConnection", "Microsoft.Data.SqlClient", "Server=.;Database=RecallFixtureEventProcessing;User Id=sa;Password=Pass!000;TrustServerCertificate=true");
            })
            .AddDataAccessLogging(builder =>
            {
                builder.Options.DatabaseContext = false;
                builder.Options.DbCommandFactory = true;
            })
            .AddSqlEventStorage(builder =>
            {
                builder.Options.ConnectionStringName = "StorageConnection";
                builder.Options.Schema = "Recall";

                builder.UseSqlServer();
            })
            .AddSqlEventProcessing(builder =>
            {
                builder.Options.ConnectionStringName = "EventProcessingConnection";
                builder.Options.Schema = "Recall";

                builder.UseSqlServer();
            })
            .AddEventStoreLogging();

        return services;
    }

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        DbProviderFactories.RegisterFactory("Microsoft.Data.SqlClient", SqlClientFactory.Instance);
    }
}