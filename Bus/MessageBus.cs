
using Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Bus
{
    public sealed class MessageBus : IMessageBus, IDisposable
    {
        private readonly IConnection _connection;
        private readonly IChannel _channel;
        private readonly JsonSerializerOptions _jsonOptions;

        public MessageBus(IConnection connection, IChannel channel)
        {
            _connection = connection;
            _channel = channel;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        public static async Task<MessageBus> CreateAsync(
            string amqpUri,
            CancellationToken ct = default
            ) 
        {
            var factory = new ConnectionFactory 
            { 
                Uri = new Uri(amqpUri),
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true
            };

            var connection = await factory.CreateConnectionAsync(ct);
            var channel = await connection.CreateChannelAsync();

            return new MessageBus(connection, channel);
        }

        public async Task PublishAsync<T>(
            T message, 
            string routingKey, 
            string exchangeName, 
            CancellationToken ct = default
            ) where T : IntegrationEvent
        {
            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            await EnsureExchangeAsync(exchangeName, ct);

            await _channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: false,
                body: body,
                basicProperties: GeraPropriedades()
            );
        }

        public async Task<TResponse> RequestAsync<TRequest, TResponse>(
            TRequest request, 
            string exchange, 
            string routingKey,
            CancellationToken ct = default
            )
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage
        {
            var correlationId = Guid.NewGuid().ToString();

            await EnsureExchangeAsync(exchange, ct);

            var replyQueueName = await CreateReplyQueueAsync(ct);

            var tcs = new TaskCompletionSource<TResponse>(
                TaskCreationOptions.RunContinuationsAsynchronously
                );

            var consumer = CreateReplyConsumer(correlationId, tcs);

            await _channel.BasicConsumeAsync(
                queue: replyQueueName,
                autoAck: true,
                consumer: consumer,
                cancellationToken: ct
            );

            await PublishRequestAsync(
                request, 
                exchange, 
                routingKey, 
                replyQueueName, 
                correlationId, 
                ct
                );

            return await tcs.Task.WaitAsync(ct);
        }

        public async Task<IDisposable> RespondAsync<TRequest, TResponse>(
                Func<TRequest, Task<TResponse>> responder,
                CancellationToken ct
            )
            where TRequest : IntegrationEvent
            where TResponse : ResponseMessage
        {
            var queueName = typeof(TRequest).Name.ToLowerInvariant();

            await EnsureExchangeAsync(queueName, ct);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                try
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var request = JsonSerializer.Deserialize<TRequest>(
                        json,
                        _jsonOptions);

                    if (request == null)
                        return;

                    var response = await responder(request);

                    await PublishResponseAsync(
                        response,
                        ea.BasicProperties?.ReplyTo,
                        ea.BasicProperties?.CorrelationId);
                }
                catch (Exception ex)
                {
                    // tratar exception
                }
            };

            var consumerTag = await _channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumer: consumer
                );

            return new ConsumerDisposable(_channel, consumerTag);
        }

        private async Task PublishResponseAsync<TResponse>(
            TResponse response,
            string replyTo,
            string correlationId
            )
        {
            if (string.IsNullOrEmpty(replyTo))
                return;

            var json = JsonSerializer.Serialize(response, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                CorrelationId = correlationId,
                ContentType = "application/json"
            };

            await _channel.BasicPublishAsync(
                exchange: "", 
                routingKey: replyTo,
                mandatory: false,
                basicProperties: props,
                body: body);
        }


        private AsyncEventingBasicConsumer CreateReplyConsumer<TResponse>(
            string correlationId, 
            TaskCompletionSource<TResponse> tcs
            )
        {
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                if (ea.BasicProperties?.CorrelationId != correlationId)
                    return;

                var json = Encoding.UTF8.GetString(ea.Body.ToArray());
                var response = JsonSerializer.Deserialize<TResponse>(json, _jsonOptions);
                if (response != null)
                    tcs.TrySetResult(response);

                await Task.CompletedTask;
            };

            return consumer;
        }

        private async Task<string> CreateReplyQueueAsync(CancellationToken ct)
        {
            var replyQueue = await _channel.QueueDeclareAsync(
                    queue: "",
                    durable: false,
                    exclusive: true,
                    autoDelete: true,
                    arguments: null,
                    cancellationToken: ct
                );

            return replyQueue.QueueName; 
        }

        private async Task PublishRequestAsync<TRequest>(
                TRequest request,
                string exchange,
                string routingKey,
                string replyQueueName,
                string correlationId,
                CancellationToken ct
            )
            where TRequest : IntegrationEvent
        {
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request, _jsonOptions));

            await _channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: GeraPropriedades(correlationId),
                body: body,
                cancellationToken: ct
            );
        }

        public void Dispose()
        {
            if (_channel != null)
                _channel.Dispose();
            if(_connection != null )
                _connection.Dispose();
        }

        internal sealed class ConsumerDisposable : IDisposable
        {
            private readonly IChannel _channel;
            private readonly string _consumerTag;

            public ConsumerDisposable(IChannel channel, string consumerTag)
            {
                _channel = channel;
                _consumerTag = consumerTag;
            }

            public void Dispose()
            {
                _channel.BasicCancelAsync(_consumerTag);
            }
        }

        private BasicProperties GeraPropriedades() =>
            new BasicProperties
            {
                ContentType = "application/json",
                ContentEncoding = "utf-8",
                DeliveryMode = DeliveryModes.Persistent
            };

        private BasicProperties GeraPropriedades(string correlationId) =>
             new BasicProperties
             {
                 CorrelationId = correlationId,
                 ContentType = "application/json",
                 ContentEncoding = "utf-8",
                 DeliveryMode = DeliveryModes.Persistent
             };

        private async Task EnsureExchangeAsync(string exchange, CancellationToken ct)
        {
            await _channel.ExchangeDeclareAsync(
                exchange: exchange,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false,
                arguments: null,
                cancellationToken: ct
            );
        }
    }
}
