using Kendo.Mvc.UI;
using Kendo.Mvc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entrvo.DAL;
using Kendo.Mvc.Extensions;

namespace Entrvo.Controllers
{
  public class ConsumersController : Controller
  {
    private readonly ApplicationDbContext _context;

    public ConsumersController(ApplicationDbContext context)
    {
      _context = context;
    }

    public IActionResult Index()
    {
      return View();
    }

    public async Task<IActionResult> _Read([DataSourceRequest] DataSourceRequest request)
    {
      var cards = await _context.Consumers.AsNoTracking().ToDataSourceResultAsync(request);
      return Json(cards);
    }

    [HttpPost]
    public ActionResult Export(string contentType, string base64, string fileName)
    {
      var fileContents = Convert.FromBase64String(base64);

      return File(fileContents, contentType, fileName);
    }
  }
}
