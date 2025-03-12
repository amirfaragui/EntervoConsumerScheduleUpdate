using EntrvoWebApp.Services;

namespace Entrvo.Services
{

  static class EntrvoServiceExtension
  {
    public static IServiceCollection AddEntrvoService(this IServiceCollection services)
    {
      services.AddScoped<IAccessProfileService, AccessProfileService>();
      services.AddScoped<IFileParser, FileParser>();
      services.AddScoped<IEntrvoConsumerService, EntrvoConsumerService>();

      services.AddSingleton<EntrvoService>();
      services.AddSingleton<IEntrvoService>(s => s.GetRequiredService<EntrvoService>());
      services.AddSingleton<IHostedService>(s => s.GetRequiredService<EntrvoService>());

      return services;
    }
  }

}
