using Entrvo.Services;

namespace Microsoft.Extensions.DependencyInjection
{
  static class FilesWatcherServiceExtension
  {
    public static IServiceCollection AddFileWatcherService(this IServiceCollection services, Action<IFilesWatchOptions> configure)
    {
      services.AddSingleton<IFilesWatchOptions>(s =>
      {
        var options = new FilesWatchOptions();
        configure(options);
        return options;
      });
      services.AddSingleton<FilesWatcherService>();
      services.AddSingleton<IFileWatcherService>(s => s.GetRequiredService<FilesWatcherService>());
      services.AddSingleton<IHostedService>(s => s.GetRequiredService<FilesWatcherService>());

      return services;
    }
  }
}