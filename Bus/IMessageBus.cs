using Messages;

namespace Bus
{
    public interface IMessageBus
    {
        Task PublishAsync<T>(
            T message,  
            CancellationToken ct = default
            ) where T : IntegrationEvent;

        Task<TResponse> RequestAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken ct = default
            )
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage;

        Task<IDisposable> RespondAsync<TRequest, TResponse>(
            Func<TRequest, Task<TResponse>> responder,
            CancellationToken ct = default
            )
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage;
            
    }
}
