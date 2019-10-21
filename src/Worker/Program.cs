using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Logging;
using Quartz.Spi;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;
using Worker.Core;
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
                var host = CreateHostBuilder(args).Build();

                await host.RunAsync()
                          .ContinueWith(task => { })
                          .ConfigureAwait(false);
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
            return new HostBuilder() //Host//.CreateDefaultBuilder(args)
                   .UseContentRoot(Directory.GetCurrentDirectory())
                   .UseWindowsService()
                   .ConfigureAppConfiguration((context, builder) =>
                   {
                       IHostEnvironment env = context.HostingEnvironment;

                       builder.AddJsonFile("appsettings.json", false, true)
                              .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                              .AddEnvironmentVariables()
                              .AddCommandLine(args);

                       if (env.IsDevelopment())
                       {
                           builder.AddUserSecrets<ServiceWorker>();
                       }

                       context.Configuration = builder.Build();
                   })
                   .ConfigureLogging((context, builder) =>
                   {
                       builder.ClearProviders();
                       builder.AddSerilog(Log.Logger);
                   })
                   .UseDefaultServiceProvider((context, options) =>
                   {
                       var isDevelopment = context.HostingEnvironment.IsDevelopment();
                       options.ValidateScopes = isDevelopment;
                       options.ValidateOnBuild = isDevelopment;
                   })
                   .ConfigureServices((hostContext, services) =>
                   {
                       services.AddOptions();
                       services.Configure<JobSchedules>(hostContext.Configuration.GetSection("JobSchedules"));
                       services.AddTransient<IJobSchedulesProvider, JobSchedulesProvider>();

                       services.AddHostedService<ServiceWorker>();

                       services.AddSingleton<IJobFactory, JobFactory>();
                       services.AddSingleton<ISchedulerFactory, SchedulerFactory>();
                       services.AddSingleton<QuartzJobRunner>();
                       services.AddSingleton<ILogProvider, QuartzLogProvider>();

                       services.AddTransient<UpdateTriggersJob>();
                       services.AddTransient<RequestBreedingValuesJob>();
                       services.AddTransient<UpdateAnimalDataJob>();
                   })
                   .ConfigureWebHostDefaults(webBuilder =>
                   {
                       webBuilder.UseStartup<Startup>();
                   });
        }
    }
}
