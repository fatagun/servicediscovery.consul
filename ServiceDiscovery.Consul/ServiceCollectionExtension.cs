using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Consul;

namespace ServiceDiscovery.Consul
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddConsul(this IServiceCollection services, IConfiguration configuration)
        {
            var options = new ConsulOptions();
            configuration.GetSection(nameof(ConsulOptions)).Bind(options);
            services.Configure<ConsulOptions>(configuration.GetSection(nameof(ConsulOptions)));

            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(consulConfig =>
            {
                var address = options.Address;
                consulConfig.Address = new Uri(address);
            }));

            return services;
        }
    }
}
