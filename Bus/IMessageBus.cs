using Messages;

namespace Bus
{
    /// <summary>
    /// Interface principal para o Message Bus utilizando RabbitMQ.
    /// Provê métodos para publicação de eventos e padrão Request/Response.
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Publica um evento de integração de forma assíncrona (Fire and Forget).
        /// </summary>
        /// <typeparam name="T">Tipo do evento que herda de IntegrationEvent.</typeparam>
        /// <param name="message">A instância do evento a ser publicado.</param>
        /// <param name="ct">Token de cancelamento.</param>
        Task PublishAsync<T>(
            T message,  
            CancellationToken ct = default
            ) where T : IntegrationEvent;

        /// <summary>
        /// Envia uma requisição e aguarda por uma resposta (Padrão RPC).
        /// </summary>
        /// <typeparam name="TRequest">Tipo da requisição.</typeparam>
        /// <typeparam name="TResponse">Tipo da resposta esperada.</typeparam>
        /// <param name="request">A instância da requisição.</param>
        /// <param name="ct">Token de cancelamento.</param>
        /// <returns>A mensagem de resposta enviada pelo processador.</returns>
        Task<TResponse> RequestAsync<TRequest, TResponse>(
            TRequest request,
            CancellationToken ct = default
            )
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage;

        /// <summary>
        /// Registra um processador (responder) para um tipo específico de requisição.
        /// </summary>
        /// <typeparam name="TRequest">Tipo da requisição a ser processada.</typeparam>
        /// <typeparam name="TResponse">Tipo da resposta a ser retornada.</typeparam>
        /// <param name="responder">Função que processa a requisição e retorna a resposta.</param>
        /// <param name="ct">Token de cancelamento.</param>
        /// <returns>Um objeto IDisposable que, ao ser descartado, cancela a inscrição do consumidor.</returns>
        Task<IDisposable> RespondAsync<TRequest, TResponse>(
            Func<TRequest, Task<TResponse>> responder,
            CancellationToken ct = default
            )
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage;
            
    }
}
