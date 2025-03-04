using Microsoft.AspNetCore.Mvc;
using Entrvo.Services;

namespace Entrvo.Controllers
{
  public class DashboardController : Controller
  {
    private readonly IFtpDownloadService _downloadService;
    private readonly IScheduleService _scheduleService;

    public DashboardController(IFtpDownloadService downloadService,
                               IScheduleService scheduleService)
    {
      _downloadService = downloadService;
      _scheduleService = scheduleService;
    }

    public IActionResult Index()
    {
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> DownloadPayrollFile()
    {
      await _scheduleService.AddPayrollFileDownloadJob();
      return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> DownloadConsumers()
    {
      await _scheduleService.AddConsumersDownloadJob();
      return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateConsumers()
    {
      await _scheduleService.AddConsumersPushJob();
      return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Generate01ExportFile()
    {
      await _scheduleService.AddExport01ReportJOb();
      return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> Generate04ExportFile()
    {
      await _scheduleService.AddExport04ReportJOb();
      return Ok();
    }
  }
}
