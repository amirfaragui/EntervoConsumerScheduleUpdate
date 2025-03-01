using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Hosting;
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

namespace Microsoft.Extensions.DependencyInjection
{
  static class KestrelServerOptionsExtensions
  {
    public static void ConfigureHttpsRedirection(this IServiceCollection services,
                                                      IConfigurationSection configuration)

    {
      var endpoints = configuration
          .GetChildren()
          .ToDictionary(section => section.Key, section =>
          {
            var endpoint = new EndpointConfiguration();
            section.Bind(endpoint);
            return endpoint;
          });

      foreach (var endpoint in endpoints)
      {
        var config = endpoint.Value;
        if (config.Scheme == "https")
        {
          services.AddHttpsRedirection(options =>
          {
            options.HttpsPort = config.Port;
            options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
          });
          break;
        }
      }
    }

    public static void ConfigureEndpoints(this KestrelServerOptions options)
    {
      var configuration = options.ApplicationServices.GetRequiredService<IConfiguration>();
      var environment = options.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
      var logger = options.ApplicationServices.GetRequiredService<ILogger<WebApplication>>();

      var endpoints = configuration.GetSection("HttpServer:Endpoints")
          .GetChildren()
          .ToDictionary(section => section.Key, section =>
          {
            var endpoint = new EndpointConfiguration();
            section.Bind(endpoint);
            return endpoint;
          });

      foreach (var endpoint in endpoints)
      {
        var config = endpoint.Value;
        var port = config.Port ?? (config.Scheme == "https" ? 443 : 80);

        var ipAddresses = new List<string>();
        if (config.Host == "localhost")
        {
          //ipAddresses.Add(IPAddress.IPv6Any);
          //ipAddresses.Add(IPAddress.Any);
          ipAddresses.Add("*");
        }
        else if (IPAddress.TryParse(config.Host, out var address))
        {
          ipAddresses.Add(address.ToString());
        }
        else
        {
          ipAddresses.Add(IPAddress.IPv6Any.ToString());
        }

        foreach (var address in ipAddresses)
        {
          var listenOptions = new Action<ListenOptions>(o =>
          {
            if (config.Scheme == "https")
            {
              try
              {
                var certificate = LoadCertificate(config, environment);
                o.UseHttps(certificate);
              }
              catch (Exception ex)
              {
                logger.LogError(ex.ToString());
              }
            }
          });

          if (address == "*")
          {
            options.ListenAnyIP(port, listenOptions);
          }
          else
          {
            options.Listen(IPAddress.Parse(address), port, listenOptions);
          }
        }
      }
    }

    private static X509Certificate2 LoadCertificate(EndpointConfiguration config, IWebHostEnvironment environment)
    {
      if (config.StoreName != null && config.StoreLocation != null)
      {
        using (var store = new X509Store(config.StoreName, Enum.Parse<StoreLocation>(config.StoreLocation)))
        {
          store.Open(OpenFlags.ReadOnly);
          var certificate = store.Certificates.Find(
              X509FindType.FindBySubjectName,
              config.Host,
              validOnly: !environment.IsDevelopment());

          if (certificate.Count == 0)
          {
            throw new InvalidOperationException($"Certificate not found for {config.Host}.");
          }

          return certificate[0];
        }
      }

      if (config.FilePath != null && config.Password != null)
      {
        var filePath = config.FilePath;
        if (!Path.IsPathRooted(filePath))
        {
          filePath = Path.Combine(environment.ContentRootPath, filePath);
        }
        return new X509Certificate2(filePath, config.Password);
      }

      throw new InvalidOperationException("No valid certificate configuration found for the current endpoint.");
    }
  }

  class EndpointConfiguration
  {
    public string Host { get; set; }
    public int? Port { get; set; }
    public string Scheme { get; set; }
    public string StoreName { get; set; }
    public string StoreLocation { get; set; }
    public string FilePath { get; set; }
    public string Password { get; set; }
  }
}
