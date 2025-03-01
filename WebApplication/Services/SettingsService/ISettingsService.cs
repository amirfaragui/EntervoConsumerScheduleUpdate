using T2WebApplication.Models;

namespace T2WebApplication.Services
{
  public interface ISettingsService
  {
    Task<SettingsModel> LoadSettingsAsync();
    Task UpdateFtpSettingsAsync(FtpSourceModel settings, string userId);
    Task UpdateApiSettingsAsync(ApiDestinationModel apiDestinationModel, string userId);
    Task SetConsumerDatabaseInitialized(bool initialized);
  }
}
