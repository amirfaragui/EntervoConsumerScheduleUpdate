using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2Importer.DAL;
using T2WebApplication.Services;

namespace T2WebApplication.Controllers
{
  public class NotesController : EntityController<CustomerNote>
  {
    private readonly ApplicationDbContext _context;


    public NotesController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<NotesController> logger, IImportingService importService): base(hostingEnvironment, logger, importService)
    {
      _context = context;
    }

    #region CustomerNotes
    public IActionResult CustomerNotes(int? id)
    {
      ViewData["CustomerId"] = id;
      return PartialView();
    }

    public async Task<IActionResult> CustomerNotes_Read([DataSourceRequest] DataSourceRequest request, int customerId)
    {
      var Notes = await _context.CustomerNotes.AsNoTracking().Where(i => i.SourceObjectUID == customerId).ToDataSourceResultAsync(request);
      return Json(Notes);
    }

    public async Task<IActionResult> CustomerNotes_Destroy([DataSourceRequest] DataSourceRequest request, CustomerNote model)
    {
      var dbItem = await _context.CustomerNotes.FirstOrDefaultAsync(i => i.SourceObjectUID == model.SourceObjectUID);
      if (dbItem != null)
      {
        _context.CustomerNotes.Remove(dbItem);
        await _context.SaveChangesAsync();
        return Json(new CustomerNote[] { dbItem }.ToDataSourceResult(request));
      }
      return NotFound();
    }

    public async Task<IActionResult> CustomerNotes_Update([DataSourceRequest] DataSourceRequest request, CustomerNote model)
    {
      var e = await _context.CustomerNotes.FirstOrDefaultAsync(i => i.SourceObjectUID == model.SourceObjectUID);
      if (e == null)
      {
        return NotFound();
      }

      e.Document = model.Document;
      e.EndDate = model.EndDate;
      e.IsHistorical = model.IsHistorical;
      e.ModifyDate = DateTime.Now;
      e.NoteText = model.NoteText;
      e.NoteType = model.NoteType;

      await _context.SaveChangesAsync().ConfigureAwait(false);

      return Json(new CustomerNote[] { e }.ToDataSourceResult(request));
    }

    public async Task<IActionResult> CustomerNotes_Create([DataSourceRequest] DataSourceRequest request, CustomerNote model, int customerId)
    {
      var noteId = 1;
      try
      {
        noteId = await _context.Notes.MaxAsync(i => i.NoteUID) + 1;
      }
      catch { }

      model.NoteUID = noteId;
      model.ModifyDate = DateTime.Now;
      model.SourceObjectUID = customerId;

      _context.CustomerNotes.Add(model);
      await _context.SaveChangesAsync().ConfigureAwait(false);

      return Json(new CustomerNote[] { model }.ToDataSourceResult(request));
    }
    #endregion

    #region PermitNotes
    public IActionResult PermitNotes(int? id)
    {
      ViewData["PermitId"] = id;
      return PartialView();
    }

    public async Task<IActionResult> PermitNotes_Read([DataSourceRequest] DataSourceRequest request, int permitId)
    {
      var Notes = await _context.PermitNotes.AsNoTracking().Where(i => i.SourceObjectUID == permitId).ToDataSourceResultAsync(request);
      return Json(Notes);
    }

    public async Task<IActionResult> PermitNotes_Destroy([DataSourceRequest] DataSourceRequest request, PermitNote model)
    {
      var dbItem = await _context.PermitNotes.FirstOrDefaultAsync(i => i.SourceObjectUID == model.SourceObjectUID);
      if (dbItem != null)
      {
        _context.PermitNotes.Remove(dbItem);
        await _context.SaveChangesAsync();
        return Json(new PermitNote[] { dbItem }.ToDataSourceResult(request));
      }
      return NotFound();
    }

    public async Task<IActionResult> PermitNotes_Update([DataSourceRequest] DataSourceRequest request, PermitNote model)
    {
      var e = await _context.PermitNotes.FirstOrDefaultAsync(i => i.SourceObjectUID == model.SourceObjectUID);
      if (e == null)
      {
        return NotFound();
      }

      e.Document = model.Document;
      e.EndDate = model.EndDate;
      e.IsHistorical = model.IsHistorical;
      e.ModifyDate = DateTime.Now;
      e.NoteText = model.NoteText;
      e.NoteType = model.NoteType;

      await _context.SaveChangesAsync().ConfigureAwait(false);

      return Json(new PermitNote[] { e }.ToDataSourceResult(request));
    }

    public async Task<IActionResult> PermitNotes_Create([DataSourceRequest] DataSourceRequest request, PermitNote model, int permitId)
    {
      var noteId = 1;
      try
      {
        noteId = await _context.Notes.MaxAsync(i => i.NoteUID) + 1;
      }
      catch { }

      model.NoteUID = noteId;
      model.ModifyDate = DateTime.Now;
      model.SourceObjectUID = permitId;

      _context.PermitNotes.Add(model);
      await _context.SaveChangesAsync().ConfigureAwait(false);

      return Json(new PermitNote[] { model }.ToDataSourceResult(request));
    }
    #endregion

  }
}
