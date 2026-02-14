using auth.Infra.Messages;

namespace auth.Infra.MessageBus
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(
            T message, 
            string routingKey, 
            string exchangeName, 
            CancellationToken ct = default
            ) where T : IntegrationEvent;

        Task<TResponse> RequestAsync<TRequest, TResponse>(
            TRequest request,
            string exchange,
            string routingKey,
            CancellationToken ct = default
            )
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage;
    }
}
