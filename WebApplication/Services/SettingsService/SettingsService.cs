using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NuGet.Configuration;
using T2Importer.DAL;
using T2WebApplication.Models;
using Telerik.SvgIcons;

namespace T2WebApplication.Services
{
  public class SettingsService : ISettingsService
  {
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<SettingsService> _logger;
    private readonly IServiceScopeFactory _scopedServiceFactory;

    const string FtpEndpointKey = "FtpEndpoint";
    const string FtpUserNameKey = "FtpUserName";
    const string FtpPasswordKey = "FtpPassword";
    const string FtpRootDirectoryKey = "FtpRootDirectory";
    const string AverageFullFileSizeKey = "AverageFullFileSize";
    const string AverageIncrementalFileSizeKey = "AverageIncrementalFileSize";

    const string SB_EndpointKey = "SB.Endpoint";
    const string SB_UserNameKey = "SB.UserName";
    const string SB_PasswordKey = "SB.Password";
    const string SB_ContractNumberKey = "SB.ContractNumber";
    const string SB_InstanceNumberKey = "SB.InstanceNumber";
    const string SB_DatabaseInitializedKey = "SB.DatabaseInitialized";



    public SettingsService(IServiceScopeFactory scopedServiceFactory,
                           IMemoryCache memoryCache,
                           ILogger<SettingsService> logger)
    {
      _memoryCache = memoryCache;
      _scopedServiceFactory = scopedServiceFactory;
      _logger = logger;
    }


    public async Task<SettingsModel> LoadSettingsAsync()
    {
      if (_memoryCache.TryGetValue<SettingsModel>("t2-settings", out var model))
      {
        if (model != null)
        {
          return model;
        }
      }

      model = new SettingsModel();
      using var scope = _scopedServiceFactory.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
      var settings = await context.Settings.ToArrayAsync();

      model.Source.Server = settings.GetValue<string>(FtpEndpointKey, "sftpflex.t2hosted.ca");
      model.Source.UserName = settings.GetValue<string>(FtpUserNameKey);
      model.Source.Password = settings.GetValue<string>(FtpPasswordKey);
      model.Source.RootDirectory = settings.GetValue<string>(FtpRootDirectoryKey);
      model.Source.AverageFullFileSize = settings.GetValue<long?>(AverageFullFileSizeKey);
      model.Source.AverageIncrementalFileSize = settings.GetValue<long?>(AverageIncrementalFileSizeKey);

      model.Destination.Server = settings.GetValue<string>(SB_EndpointKey, @"https://209.151.135.7:8443");
      model.Destination.UserName = settings.GetValue<string>(SB_UserNameKey);
      model.Destination.Password = settings.GetValue<string>(SB_PasswordKey);
      model.Destination.ContractNumber = settings.GetValue<int>(SB_ContractNumberKey);
      model.Destination.InstanceNumber = settings.GetValue<int>(SB_InstanceNumberKey);

      model.IsConsumerDatabaseInitialized = settings.GetValue<bool>(SB_DatabaseInitializedKey);

      var options = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);
      _memoryCache.Set("valet-settings", model, options);

      return model;
    }

    public async Task UpdateFtpSettingsAsync(FtpSourceModel model, string userId)
    {
      using var scope = _scopedServiceFactory.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

      var settings = await context.Settings.ToArrayAsync();

      var ftpEndpoint = settings.FirstOrDefault(i => i.Key == FtpEndpointKey);
      if (ftpEndpoint == null)
      {
        ftpEndpoint = new Setting() { Key = FtpEndpointKey };
        context.Settings.Add(ftpEndpoint);
      }
      ftpEndpoint.Value = model.Server ?? string.Empty;

      var ftpUsername = settings.FirstOrDefault(i => i.Key == FtpUserNameKey);
      if (ftpUsername == null)
      {
        ftpUsername = new Setting() { Key = FtpUserNameKey };
        context.Settings.Add(ftpUsername);
      }
      ftpUsername.Value = model.UserName ?? string.Empty;

      var ftpPassword = settings.FirstOrDefault(i => i.Key == FtpPasswordKey);
      if (ftpPassword == null)
      {
        ftpPassword = new Setting() { Key = FtpPasswordKey };
        context.Settings.Add(ftpPassword);
      }
      ftpPassword.Value = model.Password ?? string.Empty;

      var ftpRoot = settings.FirstOrDefault(i => i.Key == FtpRootDirectoryKey);
      if (ftpRoot == null)
      {
        ftpRoot = new Setting() { Key = FtpRootDirectoryKey };
        context.Settings.Add(ftpRoot);
      }
      ftpRoot.Value = model.RootDirectory ?? "/";

      var varFullSize = settings.FirstOrDefault(i => i.Key == AverageFullFileSizeKey);
      if (varFullSize == null)
      {
        varFullSize = new Setting() { Key = AverageFullFileSizeKey };
        context.Settings.Add(varFullSize);
      }
      if (model.AverageFullFileSize.HasValue)
      {
        varFullSize.Value = model.AverageFullFileSize.Value.ToString();
      }
      else
      {
        varFullSize.Value = string.Empty;
      }

      var varIncrementalSize = settings.FirstOrDefault(i => i.Key == AverageIncrementalFileSizeKey);
      if (varIncrementalSize == null)
      {
        varIncrementalSize = new Setting() { Key = AverageIncrementalFileSizeKey };
        context.Settings.Add(varIncrementalSize);
      }
      if (model.AverageIncrementalFileSize.HasValue)
      {
        varIncrementalSize.Value = model.AverageIncrementalFileSize.Value.ToString();
      }
      else
      {
        varIncrementalSize.Value = string.Empty;
      }

      await context.SaveChangesAsync(userId);

      var cached = await LoadSettingsAsync();
      if (cached != null)
      {
        cached.Source.Server = model.Server ?? string.Empty;
        cached.Source.UserName = model.UserName ?? string.Empty;
        cached.Source.Password = model.Password ?? string.Empty;
        cached.Source.RootDirectory = model.RootDirectory ?? "/";
        cached.Source.AverageFullFileSize = model.AverageFullFileSize;
        cached.Source.AverageIncrementalFileSize = model.AverageIncrementalFileSize;
      }

      var options = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);
      _memoryCache.Set("t2-settings", cached, options);
    }

    public async Task UpdateApiSettingsAsync(ApiDestinationModel model, string userId)
    {
      using var scope = _scopedServiceFactory.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

      var settings = await context.Settings.ToArrayAsync();

      var apiEndpoint = settings.FirstOrDefault(i => i.Key == SB_EndpointKey);
      if (apiEndpoint == null)
      {
        apiEndpoint = new Setting() { Key = SB_EndpointKey };
        context.Settings.Add(apiEndpoint);
      }
      apiEndpoint.Value = model.Server ?? string.Empty;

      var apiUsername = settings.FirstOrDefault(i => i.Key == SB_UserNameKey);
      if (apiUsername == null)
      {
        apiUsername = new Setting() { Key = SB_UserNameKey };
        context.Settings.Add(apiUsername);
      }
      apiUsername.Value = model.UserName ?? string.Empty;

      var apiPassword = settings.FirstOrDefault(i => i.Key == SB_PasswordKey);
      if (apiPassword == null)
      {
        apiPassword = new Setting() { Key = SB_PasswordKey };
        context.Settings.Add(apiPassword);
      }
      apiPassword.Value = model.Password ?? string.Empty;

      var contractNumber = settings.FirstOrDefault(i => i.Key == SB_ContractNumberKey);
      if (contractNumber == null)
      {
        contractNumber = new Setting() { Key = SB_ContractNumberKey };
        context.Settings.Add(contractNumber);
      }
      contractNumber.Value = model.ContractNumber.ToString();

      var instanceNumber = settings.FirstOrDefault(i => i.Key == SB_InstanceNumberKey);
      if (instanceNumber == null)
      {
        instanceNumber = new Setting() { Key = SB_InstanceNumberKey };
        context.Settings.Add(instanceNumber);
      }
      instanceNumber.Value = model.InstanceNumber.ToString();

      await context.SaveChangesAsync(userId);

      var cached = await LoadSettingsAsync();
      if (cached != null)
      {
        cached.Destination.Server = model.Server ?? string.Empty;
        cached.Destination.UserName = model.UserName ?? string.Empty;
        cached.Destination.Password = model.Password ?? string.Empty;
        cached.Destination.ContractNumber = model.ContractNumber;
        cached.Destination.InstanceNumber = model.InstanceNumber;
      }

      var options = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);
      _memoryCache.Set("t2-settings", cached, options);
    }

    public async Task SetConsumerDatabaseInitialized(bool initialized)
    {
      using var scope = _scopedServiceFactory.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

      var settings = await context.Settings.ToArrayAsync();

      var item = settings.FirstOrDefault(i => i.Key == SB_DatabaseInitializedKey);
      if (item == null)
      {
        item = new Setting() { Key = SB_DatabaseInitializedKey };
        context.Settings.Add(item);
      }
      item.Value = initialized.ToString();

      await context.SaveChangesAsync();

      var cached = await LoadSettingsAsync();
      if (cached != null)
      {
        cached.IsConsumerDatabaseInitialized = initialized;
      }

      var options = new MemoryCacheEntryOptions().SetPriority(CacheItemPriority.NeverRemove);
      _memoryCache.Set("t2-settings", cached, options);
    }
  }
}
