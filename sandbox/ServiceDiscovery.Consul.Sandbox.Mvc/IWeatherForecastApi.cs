using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Refit;

namespace ServiceDiscovery.Consul.Sandbox.Mvc
{
    public interface IWeatherForecastApi
    {
        [Get("/WeatherForecast")]
        Task<List<Weather>> GetWeather();
    }

    public class Weather
    {
        public DateTime date {get;set;}
        public int temperatureC {get;set;}
        public int temperatureF {get;set;}
        public string summary {get;set;}
    }
}