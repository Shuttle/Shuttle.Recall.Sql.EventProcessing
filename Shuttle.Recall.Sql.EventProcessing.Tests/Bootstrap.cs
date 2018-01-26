using Moq;
using Shuttle.Core.Container;
using Shuttle.Core.Contract;
using Shuttle.Core.Data;
#if (NETCOREAPP2_0 || NETSTANDARD2_0)
using Shuttle.Core.Data.SqlClient;
#endif

namespace Shuttle.Recall.Sql.EventProcessing.Tests
{
    public class Bootstrap : IComponentRegistryBootstrap
    {
        public void Register(IComponentRegistry registry)
        {
            Guard.AgainstNull(registry, nameof(registry));


        }
    }
}