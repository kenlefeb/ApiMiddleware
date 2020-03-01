using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

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
            _logger.LogInformation("Constructing the {0}", nameof(WeatherForecastController));
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            return Post(new ForecastRequest
            {
                Days = 5,
                Minimum = -20,
                Maximum = 55,
            });
        }

        [HttpPost]
        public IEnumerable<WeatherForecast> Post(ForecastRequest request)
        {
            var rng = new Random();
            var forecasts = Enumerable.Range(1, request.Days).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(request.Minimum, request.Maximum),
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