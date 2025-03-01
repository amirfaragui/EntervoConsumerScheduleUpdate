using Microsoft.AspNetCore.Mvc;
using SnB.Models;
using T2WebApplication.Models;
using T2WebApplication.Services;

namespace T2WebApplication.Controllers
{
  public class DestinationController : Controller
  {
    private readonly ISettingsService _settingsService;
    private readonly ILogger<DestinationController> _logger;

    public DestinationController(ISettingsService settings, ILogger<DestinationController> logger)
    {
      _settingsService = settings ?? throw new ArgumentNullException(nameof(settings));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
      

    public async Task<IActionResult> Index()
    {
      var settings = await _settingsService.LoadSettingsAsync();
      var model = new ApiDestinationEditModel()
      {
        Server = settings.Destination.Server,
        UserName = settings.Destination.UserName,
      };
      return View(model);
    }

    public async Task<IActionResult> UpdateConnection(ApiDestinationEditModel model, [FromServices]IApiClient client)
    {
      try
      {
        await client.Initialize(model.Server, new Credential() { Username = model.UserName, Password = model.Password });

        var devices = await client.GetDevicesAsync();

        //var settings = await _settingsService.LoadSettingsAsync();
        //settings.Destination.Server = model.Server;
        //settings.Destination.UserName = model.UserName;
        //settings.Destination.Password = model.Password;
        //settings.Destination.ContractNumber = model.ContractNumber;
        //settings.Destination.InstanceNumber = model.InstanceNumber;

        await _settingsService.UpdateApiSettingsAsync(model, string.Empty);

        return RedirectToAction("index", "dashboard");

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
