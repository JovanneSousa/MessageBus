namespace Messages
{
    public abstract class IntegrationEvent : Event
    {
        public abstract string Exchange { get; }
        public abstract string RoutingKey { get; }
    }
}
