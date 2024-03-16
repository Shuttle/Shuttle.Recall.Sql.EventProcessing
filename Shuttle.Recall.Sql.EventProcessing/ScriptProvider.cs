using Microsoft.Extensions.Options;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;

namespace Shuttle.Recall.Sql.EventProcessing
{
	public class ScriptProvider : IScriptProvider
	{
		private readonly Core.Data.IScriptProvider _scriptProvider;

		public ScriptProvider(IOptionsMonitor<ConnectionStringOptions> connectionStringOptions, IOptions<ScriptProviderOptions> options)
		{
			Guard.AgainstNull(options, nameof(options));
			Guard.AgainstNull(options.Value, nameof(options.Value));

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
			return _scriptProvider.Get(connectionStringName, scriptName);
		}
	}
}