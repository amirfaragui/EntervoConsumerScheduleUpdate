using Entrvo.Models;

namespace Entrvo.Services
{
  public interface ISettingsService
  {
    Task<SettingsModel> LoadSettingsAsync();
    Task UpdateFtpSettingsAsync(FtpSourceModel settings, string userId);
    Task UpdateApiSettingsAsync(ApiDestinationModel apiDestinationModel, string userId);
    Task UpdateDataFolderSettingsAsync(DataFolderModel ftpOptionsModel, string userId);
    Task SetConsumerDatabaseInitialized(bool initialized);
  }
}
