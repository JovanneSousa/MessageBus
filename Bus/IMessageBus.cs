using Messages;

namespace Bus
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

        Task<IDisposable> RespondAsync<TRequest, TResponse>(
            Func<TRequest, Task<TResponse>> responder,
            CancellationToken ct
            )
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage;
            
    }
}
