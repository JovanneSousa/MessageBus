using Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Configuration
{
    public static class RabbitConfig
    {
        public static async Task<IServiceCollection> AddRabbitConfiguration(
            this IServiceCollection services, 
            IConfiguration configuration
            )
        {
            var rabbit = configuration
                .GetSection("rabbit")
                .Get<RabbitSettings>();

            if (string.IsNullOrEmpty(rabbit?.Url))
                throw new InvalidOperationException("rabbit não configurado");

            services.Configure<RabbitSettings>(
                configuration.GetSection("rabbit"));

            services.AddSingleton<IEventRouteResolver, EventRouteResolver>();

            services.AddSingleton<IMessageBus>(sp =>
            {
                var resolver = sp.GetRequiredService<IEventRouteResolver>();

                var factory = new ConnectionFactory
                {
                    Uri = new Uri(rabbit.Url),
                    AutomaticRecoveryEnabled = true,
                    TopologyRecoveryEnabled = true
                };

                var connection = factory.CreateConnectionAsync().Result;
                var channel = connection.CreateChannelAsync().Result;

                return new MessageBus(connection, channel, resolver);
            });

            return services;
        }
    }
}
