using Messages;
using Microsoft.Extensions.Options;

namespace Bus
{
    public class EventRouteResolver : IEventRouteResolver
    {
        private readonly RabbitSettings _rabbitSettings;

        public EventRouteResolver(IOptions<RabbitSettings> rabbitSettings)
        {
            _rabbitSettings = rabbitSettings.Value;
        }

        public (string exchange, string routingKey, string queue) Resolve<T>()
            => (ResolveExchange<T>(), ResolveRoutingKey<T>(), ResolveQueue<T>());

        public string ResolveExchange<T>()
        {
            var eventName = typeof(T).Name;

            if (!_rabbitSettings.Exchange.TryGetValue(eventName, out var exchange))
            {
                throw new InvalidOperationException($"Exchange não configurada para {eventName}");
            }
            return exchange;
        }

        public string ResolveQueue<T>()
        {
            var eventName = typeof(T).Name;

            if (!_rabbitSettings.Queue.TryGetValue(eventName, out var queue))
            {
                throw new InvalidOperationException($"Exchange não configurada para {eventName}");
            }
            return queue;
        }

        public string  ResolveRoutingKey<T>()
        {
            var eventName = typeof(T).Name;

            if (!_rabbitSettings.RoutingKey.TryGetValue(eventName, out var routing))
                throw new InvalidOperationException($"RoutingKey não configurada para {eventName}");
            return  routing;
        }
    }
}
