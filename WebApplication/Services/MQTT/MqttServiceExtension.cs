using MQTTnet.AspNetCore;
using Entrvo.Services;

namespace Microsoft.Extensions.DependencyInjection
{

  static class MqttServiceExtension
  {
    public static IServiceCollection AddMqttService(this IServiceCollection services)
    {
      services.AddSingleton<MqttService>();
      services.AddSingleton<IMqttService>(s => s.GetService<MqttService>());

      services.AddHostedMqttServerWithServices(options =>
      {
        var s = options.ServiceProvider.GetRequiredService<MqttService>();
        s.ConfigureMqttServerOptions(options);
      })
        .AddMqttConnectionHandler()
        .AddMqttWebSocketServerAdapter()
        .AddMqttTcpServerAdapter()
        .AddConnections();

      return services;
    }
  }
}
