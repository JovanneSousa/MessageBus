using Bus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Configuration
{
    public static class RabbitConfig
    {
        public static async Task<IServiceCollection> AddRabbitConfiguration(
            this IServiceCollection services, 
            IConfiguration configuration
            )
        {
            var rabbit = configuration.GetSection("rabbit").Get<RabbitSettings>();
            if (string.IsNullOrEmpty(rabbit?.Url) || string.IsNullOrEmpty(rabbit.Exchange))
                throw new InvalidOperationException("rabbit não configurado");  
            var rabbitProducer = await MessageBus.CreateAsync(rabbit.Url);

            services.Configure<RabbitSettings>(
                configuration.GetSection("rabbit"));

            services.AddSingleton<IMessageBus>(rabbitProducer);

            return services;
        }
    }
}
