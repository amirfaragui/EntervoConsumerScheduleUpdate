using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Entrvo.Hubs;
using Entrvo.Models;
using Entrvo.Services;

namespace Entrvo.Controllers
{
  public class ProgressController: Controller
  {
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger<ProgressController> _logger;
    private readonly IImportingService _importService;
    private readonly IHubContext<ViewHub, IViewService> _progressHub;

    public ProgressController(IWebHostEnvironment hostingEnvironment,
                                    IImportingService importService,
                                    IHubContext<ViewHub, IViewService> hub,
                                    ILogger<ProgressController> logger)
    {
      _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
      _importService = importService ?? throw new ArgumentNullException(nameof(importService));
      _progressHub = hub ?? throw new ArgumentNullException(nameof(hub));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }


    [HttpGet]
    public IActionResult Index(Guid jobId)
    {
      ViewData["ActivePage"] = TempData["ActivePage"];

      if (_importService.TryGetJob(jobId, out var model))
      {
        return View(model);
      }
      return NotFound();
    }

    [HttpPost]
    public async Task<IActionResult> Start(Guid jobId, string hubConnection)
    {
      if (_importService.TryGetJob(jobId, out var model))
      {
        if (model.Status == ImportJobStatus.Created)
        {
          model.ConnectionId = hubConnection;
          await _importService.StartJob(jobId);
        }
        return Ok();
      }
      return NotFound();
    }
  }
}
