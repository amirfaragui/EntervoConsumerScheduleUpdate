using Entrvo.Services;
using Entrvo.Services.Models;

namespace Microsoft.Extensions.DependencyInjection
{
  static class ScheduleServiceExtension
  {
    public static IServiceCollection AddScheduleService(this IServiceCollection services, Action<ScheduleOptions> configure)
    {
      services.AddSingleton<ScheduleOptions>(s =>
      {
        var options = new ScheduleOptions();
        configure(options);
        return options;
      });

      services.RegisterJobDescripts();
      return services;
    }

    public static IServiceCollection AddScheduleService(this IServiceCollection services, IConfigurationSection configure)
    {
      services.Configure<ScheduleOptions>(configure);

      services.RegisterJobDescripts();
      return services;
    }

    static void RegisterJobDescripts(this IServiceCollection services)
    {
      services.AddTransient<BatchTransformJobDescriptor>();
      services.AddTransient<ConsumerDownloadJobDescriptor>();
      services.AddTransient<ConsumerUploadJobDescriptor>();
      services.AddTransient<Export01ReportJobDescriptor>();
      services.AddTransient<Export04ReportJobDescriptor>();
      services.AddTransient<FtpDownloadJobDescriptor>();

      services.AddSingleton<ScheduleService>();
      services.AddSingleton<IScheduleService>(s => s.GetRequiredService<ScheduleService>());
      services.AddSingleton<IHostedService>(s => s.GetRequiredService<ScheduleService>());
    }
  }
}