using Entrvo.Api.Models;
using Entrvo.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Collections;

namespace EntrvoWebApp.Services
{
  public class AccessProfileService : IAccessProfileService
  {
    private readonly IParkingApi _api;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AccessProfileService> _logger;

    public AccessProfileService(IParkingApi api, IMemoryCache cache, ILogger<AccessProfileService> logger)
    {
      _api = api;
      _cache = cache;
      _logger = logger;
    }

    private async Task<UsageProfile[]> GetProfilesAsync()
    {
      if (!_cache.TryGetValue("profiles", out UsageProfile[] profiles))
      {
        profiles = await _api.GetUsageProfilesAsync();
        _cache.Set("profiles", profiles, TimeSpan.FromHours(1));
      }
      return profiles;
    }

    public async Task<UsageProfile?> GetUsageProfileAsync(string profileName)
    {
      var profiles = await GetProfilesAsync();
      var match = profiles.FirstOrDefault(p => p.Name == profileName);
      if (match == null)
      {
        match = profiles.FirstOrDefault(p => p.Id == 1);
      }
      return match;
    }
  }
}
