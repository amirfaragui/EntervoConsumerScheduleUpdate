using Entrvo.Api.Models;

namespace EntrvoWebApp.Services
{
  public interface IAccessProfileService
  {
    Task<UsageProfile?> GetUsageProfileAsync(string profileName);
  }
}
