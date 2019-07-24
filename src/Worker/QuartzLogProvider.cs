using System;
using Microsoft.Extensions.Logging;
using Quartz.Logging;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;
using QuartzLogLevel = Quartz.Logging.LogLevel;

namespace Worker
{
    public class QuartzLogProvider : ILogProvider
    {
        private readonly ILogger<QuartzLogProvider> _logger;

        public QuartzLogProvider(ILogger<QuartzLogProvider> logger)
        {
            _logger = logger;
        }
        public Logger GetLogger(string name)
        {
            return (level, func, exception, parameters) =>
            {
                if (func == null) return true;

                var logLevel = level switch
                {
                    QuartzLogLevel.Trace => LogLevel.Trace,
                    QuartzLogLevel.Debug => LogLevel.Debug,
                    QuartzLogLevel.Info => LogLevel.Information,
                    QuartzLogLevel.Warn => LogLevel.Warning,
                    QuartzLogLevel.Error => LogLevel.Error,
                    QuartzLogLevel.Fatal => LogLevel.Critical,
                    _ => throw new ArgumentOutOfRangeException(nameof(level), level, null),
                };

                _logger.Log(logLevel, exception, func(), parameters);

                return true;
            };
        }

        public IDisposable OpenNestedContext(string message)
        {
            throw new NotImplementedException();
        }

        public IDisposable OpenMappedContext(string key, string value)
        {
            throw new NotImplementedException();
        }
    }
}
