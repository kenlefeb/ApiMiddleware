using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using System;

namespace Sample
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureLogging((context, options) =>
                    {
                        var instrumentationKey = context.Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
                        if (!string.IsNullOrEmpty(instrumentationKey))
                            options.AddApplicationInsights(instrumentationKey);

                        var configuration = context.Configuration.GetSection("Logging");
                        options.AddConfiguration(configuration);

                        if (!string.IsNullOrWhiteSpace(configuration["ApplicationInsights:InstrumentationKey"]))
                        {
                            options.AddApplicationInsights(configuration["ApplicationInsights:InstrumentationKey"]?.ToString() ?? "");
                            options.AddFilter<ApplicationInsightsLoggerProvider>("", Enum.Parse<LogLevel>(configuration["LogLevel:Default"] ?? "Information"));
                            options.AddFilter<ApplicationInsightsLoggerProvider>("Microsoft", Enum.Parse<LogLevel>(configuration["LogLevel:Microsoft"] ?? "Warning"));
                        }
                    });
                });
    }
}