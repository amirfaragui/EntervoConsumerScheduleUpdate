using Entrvo.Models;
using Entrvo.Services;
using Microsoft.AspNetCore.Mvc;

namespace Entrvo.Controllers
{
  public class DataFoldersController : Controller
  {
    private readonly ISettingsService _settingsService;
    private readonly IFileWatcherService _fileWatcherService;
    private readonly ILogger<DataFoldersController> _logger;

    public DataFoldersController(ISettingsService settings,
                                 IFileWatcherService fileWatcherService,
                                 ILogger<DataFoldersController> logger)
    {
      _settingsService = settings ?? throw new ArgumentNullException(nameof(settings));
      _fileWatcherService = fileWatcherService ?? throw new ArgumentNullException(nameof(fileWatcherService));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    public async Task<IActionResult> Index()
    {
      var settings = await _settingsService.LoadSettingsAsync();
      var model = new DataFolderModel()
      {
        MonitoringFolder = settings.DataFolder.MonitoringFolder,
        BackupFolder = settings.DataFolder.BackupFolder
      };
      return View("index", model);
    }

    public async Task<IActionResult> Update(DataFolderModel model)
    {
      try
      {
        if (Directory.Exists(model.MonitoringFolder) == false)
        {
          ModelState.AddModelError("MonitoringFolder", "Monitoring folder does not exist.");
          throw new Exception("Monitoring folder does not exist.");
        }
        if (Directory.Exists(model.BackupFolder) == false)
        {
          ModelState.AddModelError("BackupFolder", "Backup folder does not exist.");
          throw new Exception("Backup folder does not exist.");
        }

        await _settingsService.UpdateDataFolderSettingsAsync(model, string.Empty);

        _fileWatcherService.ChangeMonitoringFolder(model.MonitoringFolder);

        var settings = await _settingsService.LoadSettingsAsync();

        return View("index", model);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());

        ViewData["Status"] = "error";
        ViewData["Message"] = ex.Message;

        return View("index", model);
      }
    }

  }
}
