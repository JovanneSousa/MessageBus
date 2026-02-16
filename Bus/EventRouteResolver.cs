using Bus;
using Messages;
using Microsoft.Extensions.Options;
namespace MessageBus.Bus
{
    public class EventRouteResolver : IEventRouteResolver
    {
        private readonly RabbitSettings _rabbitSettings;

        public EventRouteResolver(IOptions<RabbitSettings> rabbitSettings)
        {
            _rabbitSettings = rabbitSettings.Value;
        }

        public (string exchange, string routingKey) Resolve(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name;

            if (!_rabbitSettings.Exchange.TryGetValue(eventName, out var exchange))
            {
                throw new InvalidOperationException($"Exchange não configurada para {eventName}")
            }

            if (!_rabbitSettings.RoutingKey.TryGetValue(eventName, out var routing))
                throw new InvalidOperationException($"RoutingKey não configurada para {eventName}");

            return (exchange, routing);
        }
    }
}
