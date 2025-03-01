using Microsoft.AspNetCore.Identity.UI.Services;
using T2WebApplication.Services;
using T2WebApplication.Services.Models;

namespace Microsoft.Extensions.DependencyInjection
{

  static class EmailServiceExtension
  {
    public static IServiceCollection AddEmailService(this IServiceCollection services, IConfigurationSection options)
    {
      services.Configure<EmailOptions>(options);
      services.AddSingleton<IEmailSender, EmailService>();
      services.AddSingleton<IEmailService, EmailService>(x => (EmailService)x.GetService<IEmailSender>());
      return services;
    }
  }
}
