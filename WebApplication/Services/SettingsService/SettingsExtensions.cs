using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using System.ComponentModel;
using Entrvo.DAL;
using Entrvo.Api;

namespace Entrvo.Services
{

  static class SettingsExtension
  {
    public static T GetValue<T>(this IEnumerable<Setting> settings, string key, T defaultValue = default)
    {
      var value = settings.FirstOrDefault(i => i.Key == key)?.Value;

      if (value == null) { return defaultValue; }
      try
      {
        var type = typeof(T);
        if (type.IsPrimitive || type == typeof(string))
        {
          var converter = TypeDescriptor.GetConverter(typeof(T));
          return (T)converter.ConvertFromString(value);
        }
        else
        {
          var nullableType = Nullable.GetUnderlyingType(type);
          if (nullableType != null)
          {
            type = nullableType;
          }
          if (type == typeof(TimeSpan) || type == typeof(Guid))
          {
            var jsonValue = $"\"{value}\"";
            var tvalue = JsonConvert.DeserializeObject<T>(jsonValue);
            return tvalue;
          }
          if (type.IsEnum)
          {
            if (Enum.TryParse(type, value, out var evalue))
            {
              return (T)evalue;
            }
          }
          return JsonConvert.DeserializeObject<T>(value);
        }
      }
      catch (Exception ex)
      {
        return defaultValue;
      }

    }

    public static IServiceCollection AddSettingsService(this IServiceCollection services)
    {
      //services.TryAddSingleton<ISettingsService, SettingsService>();
      //var descriptor = new ServiceDescriptor(typeof(ISettingsService), typeof(SettingsService), ServiceLifetime.Singleton);
      //services.Replace(descriptor);

      services.AddSingleton<SettingsService>();
      services.AddSingleton<IHostedService>(s => s.GetRequiredService<SettingsService>());
      services.AddSingleton<ISettingsService>(s => s.GetRequiredService<SettingsService>());
      services.AddSingleton<IApiSettingsProvider>(s => s.GetRequiredService<SettingsService>());

      return services;
    }
  }

}
