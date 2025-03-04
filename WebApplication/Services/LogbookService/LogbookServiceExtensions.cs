using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Entrvo.Services
{

  public static class LogbookServiceExtensions
  {
    public static LoggerConfiguration LogbookService<T>(this LoggerSinkConfiguration loggerConfiguration, T sink) where T : ILogEventSink, ILogbookService
    {
      return loggerConfiguration.Sink(sink, LogEventLevel.Debug);
    }

    public static IServiceCollection AddChannelSink(this IServiceCollection services, LogbookService loogbokService)
    {
      services.AddSingleton<ILogbookService>(loogbokService);
      return services;
    }

  }
}
