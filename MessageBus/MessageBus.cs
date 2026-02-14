using auth.Infra.MessageBus;
using auth.Infra.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Infra.MessageBus
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
                basicProperties: geraPropriedades()
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

            var replyQueue = await _channel.QueueDeclareAsync(
                 queue: "",
                 durable: false,
                 exclusive: true,
                 autoDelete: true,
                 arguments: null,
                 cancellationToken: ct
             );

            var tcs = new TaskCompletionSource<TResponse>(
                TaskCreationOptions.RunContinuationsAsynchronously);

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                if (ea.BasicProperties?.CorrelationId == correlationId)
                {
                    var json = Encoding.UTF8.GetString(ea.Body.ToArray());

                    var response = JsonSerializer.Deserialize<TResponse>(json, _jsonOptions);

                    if (response != null)
                        tcs.TrySetResult(response);
                }

                await Task.CompletedTask;
            };

            await _channel.BasicConsumeAsync(
                queue: replyQueue.QueueName,
                autoAck: true,
                consumer: consumer,
                cancellationToken: ct
            );

            var messageJson = JsonSerializer.Serialize(request, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(messageJson);

            var properties = new BasicProperties
            {
                CorrelationId = correlationId,
                ReplyTo = replyQueue.QueueName,
                ContentType = "application/json",
                Type = request.MessageType
            };

            await _channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: ct
            );

            return await tcs.Task.WaitAsync(ct);
        }

        public void Dispose()
        {
            if (_channel != null)
                _channel.Dispose();
            if(_connection != null )
                _connection.Dispose();
        }


        private BasicProperties geraPropriedades() =>
            new BasicProperties
            {
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
