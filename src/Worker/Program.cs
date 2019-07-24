using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Worker.Jobs;

namespace Worker
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                         .Enrich.FromLogContext()
                         .MinimumLevel.Information()
                         .WriteTo.Console(
                             outputTemplate:
                             "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                             theme: AnsiConsoleTheme.Literate)
                         .CreateLogger();

            try
            {
                await CreateHostBuilder(args).Build().RunAsync();
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, e.Message);
                return -1;
            }

            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .UseWindowsService()
                       .ConfigureLogging(builder =>
                       {
                           builder.ClearProviders();
                           builder.AddSerilog(Log.Logger);
                       })
                       .ConfigureServices((hostContext, services) =>
                       {
                           services.AddHostedService<ServiceWorker>();

                           services.AddSingleton<IJobFactory, JobFactory>();
                           services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
                           services.AddSingleton<QuartzJobRunner>();

                           services.AddTransient<HelloWorldJob>();
                           services.AddSingleton(new JobSchedule(typeof(HelloWorldJob), "0/5 * * * * ?"));
                       });
        }
    }
}
