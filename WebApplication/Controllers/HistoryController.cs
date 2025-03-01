using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2Importer.DAL;
using T2WebApplication.Models;

namespace T2WebApplication.Controllers
{
  public class HistoryController : Controller
  {
    private readonly ApplicationDbContext _context;
    private IWebHostEnvironment _environment;
    public HistoryController(ApplicationDbContext context, IWebHostEnvironment environment)
    {
      _context = context;
      _environment = environment;
    }

    public IActionResult Index()
    {
      return View();
    }

    [HttpGet]
    public async Task<IActionResult> _Read()
    {
      var cutoffDay = DateTime.Today.AddDays(1);
      var history = await _context.Events.Where(i => EF.Functions.DateDiffDay(i.Time, cutoffDay) <= 90)
        .OrderByDescending(i => i.Time).ToArrayAsync();

      var result = history.Select(i => new History()
      {
        Id = i.Id,
        EventTime = i.Time,
        Title = i.Message ?? string.Empty,
        Description = i.Details ?? string.Empty,
        Url = GetRelativePath(i.FileUrl),
      }).ToArray();

      foreach (var item in result.Where(i => !string.IsNullOrEmpty(i.Url)))
      {
        item.Actions = new[] { new HistoryAction { text = "Download", url = Url.Action("download", "history", new { id = item.Id }) } };
      }

      return Json(result);

    }

    [HttpGet]
    public async Task<IActionResult> Download(int  id)
    {
      var e = await _context.Events.FindAsync(id);
      if (e == null || string.IsNullOrWhiteSpace(e.FileUrl)) return NotFound();

      var fileName = Path.GetFileName(e.FileUrl);
      var ext = Path.GetExtension(e.FileUrl);

      var type = "html/text";
      if (ext.StartsWith(".xls"))
      {
        type = "pplication/vnd.ms-excel";
      }

      var ms = new MemoryStream();
      using var reader = System.IO.File.OpenRead(e.FileUrl);
      reader.CopyTo(ms);
      ms.Position = 0;
      return File(ms, type, fileName);      
    }

    private string GetRelativePath(string? path)
    {
      if (string.IsNullOrEmpty(path)) return string.Empty;

      var dataFolder = Path.Combine(_environment.WebRootPath, "App_Data");

      return path.Replace(dataFolder, string.Empty).Replace("\\", "/");
    }

  }
}
