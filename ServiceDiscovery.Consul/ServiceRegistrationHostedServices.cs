using System;
using System.Threading;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using Consul;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Linq;

namespace ServiceDiscovery.Consul
{
    public class ServiceRegistrationHostedService : IHostedService
    {
        private CancellationTokenSource _cts;
        private readonly IConsulClient _consulClient;
        private readonly IOptions<ConsulOptions> _consulOptions;
        private readonly ILogger<ServiceRegistrationHostedService> _logger;
        private IServer _server;
        private string _registrationId;

        public ServiceRegistrationHostedService(
                IConsulClient consulClient, 
                IServer server,
                ILogger<ServiceRegistrationHostedService> logger, 
                IOptions<ConsulOptions> consulOptions)
        {
            _consulClient = consulClient;
            _consulOptions = consulOptions;
            _logger = logger;
            _server = server;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var features = _server.Features;
            var serverAddressesFeature = features.Get<IServerAddressesFeature>();
            var address = serverAddressesFeature.Addresses.FirstOrDefault();

            if(address == null)
            {
                _logger.LogError("Server address is NULL. Check server address.");
                throw new NullReferenceException("Address is NULL");
            }

            var uri = new Uri(address);
            // Assembly / Solution 

            _registrationId = $"{_consulOptions.Value.ServiceId}";

            var registration = new AgentServiceRegistration()
            {
                ID = _registrationId,
                Name = _consulOptions.Value.ServiceName,
                Address = $"{uri.Scheme}://{uri.Host}",
                Port = uri.Port
            };

            _logger.LogInformation("Registering in Consul");
            await _consulClient.Agent.ServiceDeregister(registration.ID, _cts.Token);
            await _consulClient.Agent.ServiceRegister(registration, _cts.Token);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();
            _logger.LogInformation($"Deregistering from Consul: {_registrationId}");
            try
            {
                await _consulClient.Agent.ServiceDeregister(_registrationId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Deregisteration failed: {_registrationId}");
            }
        }
    }
}
