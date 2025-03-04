using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entrvo.DAL;
using Entrvo.Services;

namespace Entrvo.Controllers
{
  public class EmailsController : EntityController<Email>
  {
    private readonly ApplicationDbContext _context;


    public EmailsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<EmailsController> logger, IImportingService importService): base(hostingEnvironment, logger, importService)
    {
      _context = context;
    }

    #region List
    public IActionResult Index(int? id)
    {
      ViewData["CustomerId"] = id;
      return PartialView();
    }

    public async Task<IActionResult> _Read([DataSourceRequest] DataSourceRequest request, int customerId)
    {
      var emails = await _context.Emails.AsNoTracking().Where(i => i.CustomerUID == customerId).ToDataSourceResultAsync(request);
      return Json(emails);
    }

    public async Task<IActionResult> _Destroy([DataSourceRequest] DataSourceRequest request, Email model)
    {
      var dbItem = await _context.Emails.FindAsync(model.CustomerUID);
      if (dbItem != null)
      {
        _context.Emails.Remove(dbItem);
        await _context.SaveChangesAsync();
        return Json(new Email[] { dbItem }.ToDataSourceResult(request));
      }
      return NotFound();
    }

    public async Task<IActionResult> _Update([DataSourceRequest] DataSourceRequest request, Email model)
    {
      var e = await _context.Emails.FindAsync(model.EmailAddressUID);
      if (e == null)
      {
        return NotFound();
      }

      e.EffectiveRank = model.EffectiveRank;
      e.EmailAddress = model.EmailAddress;
      e.EndDate = model.EndDate;
      e.IsActive = model.IsActive;
      e.IsHistorical = model.IsHistorical;
      e.ModifyDate = DateTime.Now;
      e.Priority = model.Priority;
      e.SourceType = model.SourceType;
      e.StartDate = model.StartDate;
      e.Type = model.Type;

      await _context.SaveChangesAsync().ConfigureAwait(false);

      return Json(new Email[] { e }.ToDataSourceResult(request));
    }

    public async Task<IActionResult> _Create([DataSourceRequest] DataSourceRequest request, Email model, int customerId)
    {      
      var emailId = 1;
      try
      {
        emailId = (await _context.Emails.MaxAsync(i => i.EmailAddressUID)) + 1;
      }
      catch { }

      model.CustomerUID = customerId;
      model.EmailAddressUID = emailId;
      model.ModifyDate = DateTime.Now;

      _context.Emails.Add(model);
      await _context.SaveChangesAsync().ConfigureAwait(false);

      return Json(new Email[] { model }.ToDataSourceResult(request));
    }
    #endregion

  }
}
