using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
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
            //Log.Logger = new LoggerConfiguration()
            //            .Enrich.FromLogContext()
            //            .MinimumLevel.Information()
            //            .WriteTo.Console(
            //                             outputTemplate:
            //                             "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
            //                             theme: AnsiConsoleTheme.Literate)
            //            .CreateLogger();

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

        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder()
                  .UseWindowsService()
                  .ConfigureAppConfiguration((context, builder) =>
                   {
                       IHostEnvironment env = context.HostingEnvironment;

                       builder.AddJsonFile("appsettings.json", false, false)
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

                       Log.Logger = new LoggerConfiguration()
                                   .Enrich.FromLogContext()
                                   .MinimumLevel.Information()
                                   .WriteTo.File(path: Path.Combine(context.HostingEnvironment.ContentRootPath, "log.txt"),
                                                 rollingInterval: RollingInterval.Day)
                                   .WriteTo.Console(
                                                    outputTemplate:
                                                    "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}",
                                                    theme: AnsiConsoleTheme.Literate)
                                   .CreateLogger();


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
                       // Core
                       services.AddOptions();
                       services.Configure<JobSchedules>(hostContext.Configuration.GetSection("JobSchedules"));
                       services.AddScoped<IJobSchedulesProvider, JobSchedulesProvider>();
                       services.AddHostedService<ServiceWorker>();

                       // Quartz
                       services.AddSingleton<IJobFactory, JobFactory>();
                       services.AddSingleton<ISchedulerFactory, SchedulerFactory>();
                       services.AddSingleton<QuartzJobRunner>();
                       services.AddSingleton<ILogProvider, QuartzLogProvider>();

                       // Jobs
                       services.AddTransient<MakeSnapshotJob>();

                       // Service agents
                       services.AddHttpClient<IMakeSnapshotServiceAgent, MakeSnapshotServiceAgent>(x => x.BaseAddress = hostContext.Configuration.GetValue<Uri>("Url"))
                       //    .ConfigurePrimaryHttpMessageHandler(() =>
                       //    {
                       //        return new HttpClientHandler
                       //        {
                       //            Proxy = new System.Net.WebProxy("http://192.168.199.2:8080"),
                       //            UseProxy = true
                       //        };
                       //    })
                       ;
                   });
        }
    }
}
