using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Refit;
using ServiceDiscovery.Consul.Sandbox.Mvc.Models;


// Refit registration methods
// 1 refit registration esnasinda, uygulama start olurken, yada lazy, 1..N (N load balancing)
// delegatinghandler ile, uygulama calisirken, her N dakika da bir service discovery, 1 sefer aiyoruz cacheliyoruz, N dakika sonra tekrar aliyoruz, fresh copy aliyoruz. 


//Service registary resolving
// 
namespace ServiceDiscovery.Consul.Sandbox.Mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IConsulClient _consulClient;

        public HomeController(ILogger<HomeController> logger, IConsulClient consulClient)
        {
            _logger = logger;
            _consulClient = consulClient;
        }

        public async Task<IActionResult> Index()
        {

            var services = await _consulClient.Agent.Services();
            var serviceResponse = services.Response;

            foreach(var service in serviceResponse)
            {
                Console.WriteLine(service.Key);
                Console.WriteLine(service.Value.Address);
                Console.WriteLine(service.Value.Port);
            }

            var add = serviceResponse.FirstOrDefault(e=> e.Key == "servicediscovery.consul.sandbox.api");

            var serviceaccessurl = add.Value.Address+":"+add.Value.Port;

            var apiclient = RestService.For<IWeatherForecastApi>(serviceaccessurl);
            var weather = await apiclient.GetWeather();

            Console.WriteLine(weather.First().temperatureC);
            Console.WriteLine(weather.First().temperatureF);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
