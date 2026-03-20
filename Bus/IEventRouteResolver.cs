namespace Bus
{
    /// <summary>
    /// Define as operações para resolver rotas (Exchange, Queue, RoutingKey) baseadas no tipo da mensagem.
    /// </summary>
    public interface IEventRouteResolver
    {
        /// <summary>
        /// Resolve a tripla de roteamento (Exchange, RoutingKey, Queue) para um tipo de evento.
        /// </summary>
        /// <typeparam name="T">O tipo do evento.</typeparam>
        /// <returns>Uma tupla contendo Exchange, RoutingKey e Queue.</returns>
        (string exchange, string routingKey, string queue) Resolve<T>();

        /// <summary>
        /// Resolve apenas o nome da Exchange para o evento.
        /// </summary>
        string ResolveExchange<T>();

        /// <summary>
        /// Resolve apenas a RoutingKey para o evento.
        /// </summary>
        string ResolveRoutingKey<T>();

        /// <summary>
        /// Resolve apenas o nome da Queue para o evento.
        /// </summary>
        string ResolveQueue<T>();
    }
}
