using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Sample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly TelemetryClient _telemetry;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, TelemetryClient telemetry)
        {
            _logger = logger;
            _telemetry = telemetry;
            _logger.LogInformation($"Constructing the {nameof(WeatherForecastController)}");
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            using var operation = _telemetry.StartOperation<RequestTelemetry>("GET WeatherForecasts");
            var rng = new Random();
            var forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
                .ToArray();
            foreach (var forecast in forecasts)
            {
                _telemetry.TrackEvent($"Forecast: {forecast.Date}", new Dictionary<string, string>
                                                                    {
                                                                        {"Date", $"{forecast.Date:o}"},
                                                                        {"Celsius", $"{forecast.TemperatureC}℃"},
                                                                        {"Fahrenheit", $"{forecast.TemperatureC}℉"},
                                                                        {"Summary", $"{forecast.Summary}"},
                                                                    },
                    new Dictionary<string, double>
                    {
                        {"Forecasts", 1},
                        {"Temperature", forecast.TemperatureC }
                    });
            }
            return forecasts;
        }
    }
}
