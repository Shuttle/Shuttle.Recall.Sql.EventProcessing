using Shuttle.Core.Container;
using Shuttle.Core.Contract;

namespace Shuttle.Recall.Sql.EventProcessing
{
    public static class Extensions
    {
        public static IComponentResolver AddEventHandler<TEventHandler>(this IComponentResolver resolver, string name)
            where TEventHandler : class
        {
            Guard.AgainstNull(resolver, nameof(resolver));
            Guard.AgainstNullOrEmptyString(name, nameof(name));

            resolver
                .Resolve<IProjectionProvider>()
                .Get(name)
                .AddEventHandler(resolver.Resolve<TEventHandler>());

            return resolver;
        }

        public static IComponentResolver AddEventHandler<TEventHandler>(this IComponentResolver resolver, Projection projection)
            where TEventHandler : class
        {
            Guard.AgainstNull(resolver, nameof(resolver));
            Guard.AgainstNull(projection, nameof(projection));

            projection.AddEventHandler(resolver.Resolve<TEventHandler>());

            return resolver;
        }

    }
}