using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
using Shuttle.Recall.Sql.Storage;

namespace Shuttle.Recall.Sql.EventProcessing;

public class ScriptProvider : IScriptProvider
{
    private readonly SqlEventProcessingOptions _sqlEventProcessingOptions;
    private readonly Core.Data.IScriptProvider _scriptProvider;

    public ScriptProvider(IOptionsMonitor<ConnectionStringOptions> connectionStringOptions, IOptions<ScriptProviderOptions> options, IOptions<SqlEventProcessingOptions> sqlEventProcessingOptions)
    {
        Guard.AgainstNull(Guard.AgainstNull(options).Value);
        _sqlEventProcessingOptions = Guard.AgainstNull(Guard.AgainstNull(sqlEventProcessingOptions).Value);

        _scriptProvider = new Core.Data.ScriptProvider(connectionStringOptions, Options.Create(new ScriptProviderOptions
        {
            ResourceNameFormat = string.IsNullOrEmpty(options.Value.ResourceNameFormat)
                ? "Shuttle.Recall.Sql.EventProcessing..scripts.{ProviderName}.{ScriptName}.sql"
                : options.Value.ResourceNameFormat,
            ResourceAssembly = options.Value.ResourceAssembly ?? typeof(ProjectionRepository).Assembly,
            FileNameFormat = options.Value.FileNameFormat,
            ScriptFolder = options.Value.ScriptFolder
        }));
    }

    public string Get(string connectionStringName, string scriptName)
    {
        return _scriptProvider.Get(connectionStringName, scriptName).Replace("{schema}", _sqlEventProcessingOptions.Schema);
    }
}