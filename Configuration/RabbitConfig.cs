using Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

namespace Configuration
{
    /// <summary>
    /// Classe de configuração para integrar o RabbitMQ ao container de dependências do .NET.
    /// </summary>
    public static class RabbitConfig
    {
        /// <summary>
        /// Registra as configurações do RabbitMQ e as instâncias de <see cref="IMessageBus"/> e <see cref="IEventRouteResolver"/>.
        /// </summary>
        /// <param name="services">A coleção de serviços do .NET.</param>
        /// <param name="configuration">Instância de <see cref="IConfiguration"/> contendo a seção 'rabbit'.</param>
        /// <returns>A coleção de serviços atualizada.</returns>
        /// <exception cref="InvalidOperationException">Lançada se a URL do RabbitMQ não for encontrada no arquivo de configuração.</exception>
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
