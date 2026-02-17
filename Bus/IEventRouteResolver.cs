using Messages;

namespace Bus
{
    public interface IEventRouteResolver
    {
        (string exchange, string routingKey, string queue) Resolve<T>();
        string ResolveExchange<T>();
        string ResolveRoutingKey<T>();
        string ResolveQueue<T>();
    }
}
