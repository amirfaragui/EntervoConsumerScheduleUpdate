using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.SqlServer.Query.Internal;
using Renci.SshNet;
using Renci.SshNet.Async;
using T2WebApplication.Models;
using T2WebApplication.Services;

namespace T2WebApplication.Controllers
{
  public class FtpSourceController : Controller
  {
    private readonly ISettingsService _settingsService;
    private readonly ILogger<FtpSourceController> _logger;

    public FtpSourceController(ISettingsService settings, ILogger<FtpSourceController> logger)
    {
      _settingsService = settings ?? throw new ArgumentNullException(nameof(settings));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
      

    public async Task<IActionResult> Index()
    {
      var settings = await _settingsService.LoadSettingsAsync();
      var model = new FtpSourceEditModel()
      {
        Server = settings.Source.Server,
        UserName = settings.Source.UserName,
      };
      return View("connection", model);
    }

    public async Task<IActionResult> UpdateConnection(FtpSourceEditModel model)
    {
      using (var sftp = new SftpClient(model.Server, model.UserName, model.Password))
      {
        try
        {
          sftp.Connect();

          await _settingsService.UpdateFtpSettingsAsync(model, string.Empty);

          var settings = await _settingsService.LoadSettingsAsync();

          var files = await sftp.ListDirectoryAsync("/");
          var folders = new List<string>() { "/" };
          folders.AddRange(files.Where(i => i.IsDirectory).Select(i => i.Name).Order());
          var directoryModel = new FtpOptionsModel()
          {
            RootDirectory = settings.Source.RootDirectory,
            Folders = folders.ToArray()
          };

          sftp.Disconnect();
          return View("options", directoryModel);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());

          ViewData["Status"] = "error";
          ViewData["Message"] = ex.Message;

          return View("connection", model);
        }
      }
    }

    public async Task<IActionResult> UpdateRootDirectory(FtpOptionsModel model)
    {
      var settings = await _settingsService.LoadSettingsAsync();
      using (var sftp = new SftpClient(settings.Source.Server, settings.Source.UserName, settings.Source.Password))
      {
        try
        {
          sftp.Connect();
          var rootFolder = "/" + model.RootDirectory.Trim('/');
          var files = await sftp.ListDirectoryAsync(rootFolder);

          settings.Source.RootDirectory = rootFolder;
          await _settingsService.UpdateFtpSettingsAsync(settings.Source, string.Empty);

          var archieveFolder = files.FirstOrDefault(i => i.IsDirectory && i.Name.ToLower() == "archieve");
          if (archieveFolder != null)
          {
            var archievePath = rootFolder + "/Archieve";
            files = await sftp.ListDirectoryAsync(archievePath);
            files = files.Where(i => i.IsDirectory == false && i.Attributes.Size > 0).ToArray();

            var min = files.Min(i => i.Attributes.Size);
            var max = files.Max(i => i.Attributes.Size);

            var halfSize = (min + max) / 2;
            var incrementalFiles = files.Where(i => i.Attributes.Size < halfSize).ToArray();
            var fullFiles = files.Where(i => i.Attributes.Size >= halfSize).ToArray();


            settings.Source.AverageFullFileSize = (long)fullFiles.Average(i => i.Attributes.Size);
            settings.Source.AverageIncrementalFileSize = (long)incrementalFiles.Average(i => i.Attributes.Size);
            await _settingsService.UpdateFtpSettingsAsync(settings.Source, string.Empty);
          }

          return RedirectToAction("index", "dashboard");
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());

          ViewData["Status"] = "error";
          ViewData["Message"] = ex.Message;

          return View("options", model);
        }
      }
    }
  }
}
