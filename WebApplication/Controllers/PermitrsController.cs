using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Entrvo.DAL;
using T2WebApplication.Services;

namespace T2WebApplication.Controllers
{
  public class PermitsController : EntityController<Permit>
  {
    private readonly ApplicationDbContext _context;


    public PermitsController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<PermitsController> logger, IImportingService importService) : base(hostingEnvironment, logger, importService)
    {
      _context = context;
    }

    #region List
    public IActionResult Index(int? customerId)
    {
      if (customerId == null)
      {
        return View();
      }
      ViewData["CustomerId"] = customerId;
      return PartialView();
    }

    public IActionResult CustomerPermits(int? id)
    {
      ViewData["CustomerId"] = id;
      return PartialView();
    }

    public async Task<IActionResult> _Read([DataSourceRequest] DataSourceRequest request, int? customerId)
    {
      IQueryable<Permit> permits = _context.Permits;
      if (customerId.HasValue)
      {
        permits = permits.Where(i => i.PurchasingCustomer == customerId);
      }
      var result = await permits.ToDataSourceResultAsync(request);
      return Json(result);
    }

    public async Task<IActionResult> _ReadWithCustomer([DataSourceRequest] DataSourceRequest request)
    {
      var permits = _context.Permits.Include(e => e.Customer);

      var result = await permits.ToDataSourceResultAsync(request, i => new PermitWithCustomerName()
      {
        CustomerName = i.Customer?.FullName,
        Current = i.Current,
        PermitUID = i.PermitUID,
        ControlGroup = i.ControlGroup,
        EffectiveDate = i.EffectiveDate,
        ExpirationDate = i.ExpirationDate,
        IssueNumber = i.IssueNumber,
        Comment = i.Comment,
        IsDeactivated = i.IsDeactivated,
        IsDestroyed = i.IsDestroyed,
        IsReturned = i.IsReturned,
        IsTerminated = i.IsTerminated,
        IsMissing = i.IsMissing,
        IsMissingfromCustody = i.IsMissingfromCustody,
        MissingDate = i.MissingDate,
        MissingReason = i.MissingReason,
        ModifyDate = i.ModifyDate,
        PermitNumber = i.PermitNumber,
        PermitSeriesType = i.PermitSeriesType,
        PermitDirectStatus = i.PermitDirectStatus,
        PermitAllotment = i.PermitAllotment,
        PermitAmountDue = i.PermitAmountDue,
        PermitNumberRange = i.PermitNumberRange,
        BulkPermit = i.BulkPermit,
        CardMode = i.CardMode,
        Custody = i.Custody,
        CustodyType = i.CustodyType,
        DeactivateReason = i.DeactivateReason,
        DeactivatedDate = i.DeactivatedDate,
        Deallocator = i.Deallocator,
        DepositAmountPaid = i.DepositAmountPaid,
        DepositFee = i.DepositFee,
        Drawer = i.Drawer,
        EmailNotificationAddress = i.EmailNotificationAddress,
        ExporttoWPSRequired = i.ExporttoWPSRequired,
        FeeSchedAtPurch = i.FeeSchedAtPurch,
        HasDepositbeenrefunded = i.HasDepositbeenrefunded,
        HasLetter = i.HasLetter,
        HasNote = i.HasNote,
        HasPendingLetter = i.HasPendingLetter,
        IsEmailNotifyReq = i.IsEmailNotifyReq,
        AllocateStartDate = i.AllocateStartDate,
        AllocateReturnDate = i.AllocateReturnDate,
        Allocator = i.Allocator,
        AnonymousUserID = i.AnonymousUserID,
        AddedValue = i.AddedValue,
        MaximumValue = i.MaximumValue,
        LaneControllerStampDate = i.LaneControllerStampDate,
        LastExportedtoWPS = i.LastExportedtoWPS,
        MailTrackingNumber = i.MailTrackingNumber,
        MailingAddress = i.MailingAddress,
        MailingDate = i.MailingDate,
        IsFulfilling = i.IsFulfilling,
        IsHistorical = i.IsHistorical,
        IsMailingReq = i.IsMailingReq,
        IsPermitDirectFulfilled = i.IsPermitDirectFulfilled,
        IsPossConfReq = i.IsPossConfReq,
        IsReturningtoMainInventory = i.IsReturningtoMainInventory,
        PendingExpirationDate = i.PendingExpirationDate,
        PemissionRawNumber = i.PemissionRawNumber,
        AccessGroup = i.AccessGroup,
        ActiveCredentialID = i.ActiveCredentialID,
        PhysicalGroupType = i.PhysicalGroupType,
        PossessionDate = i.PossessionDate,
        PurchasingCustomer = i.PurchasingCustomer,
        PurchasingThirdParty = i.PurchasingThirdParty,
        RenewalGracePeriodEndingDate = i.RenewalGracePeriodEndingDate,
        RenewalStatus = i.RenewalStatus,
        ReplacementPermit = i.ReplacementPermit,
        ReservationNoShowCutoffDate = i.ReservationNoShowCutoffDate,
        RenewalUID = i.RenewalUID,
        Reserved = i.Reserved,
        ReserveEndDate = i.ReserveEndDate,
        ReserveHold = i.ReserveHold,
        ReserveStartDate = i.ReserveStartDate,
        ReturnDate = i.ReturnDate,
        ReturnReason = i.ReturnReason,
        ShippingMethod = i.ShippingMethod,
        SoldDate = i.SoldDate,
        StallId = i.StallId,
        StallType = i.StallType,
        Status = i.Status,
        TerminatedDate = i.TerminatedDate,
        WorkflowStatus = i.WorkflowStatus,
      });
      return Json(result);
    }

    public async Task<IActionResult> _Destroy([DataSourceRequest] DataSourceRequest request, Permit model)
    {
      var dbItem = await _context.Permits.FindAsync(model.PermitUID);
      if (dbItem != null)
      {
        _context.Permits.Remove(dbItem);
        await _context.SaveChangesAsync();
        return Json(dbItem);
      }
      return NotFound();
    }
    #endregion

    #region Edit
    public async Task<IActionResult> Edit(int id)
    {
      var permit = await _context.Permits.FindAsync(id);
      if (permit == null)
      {
        return NotFound();
      }
      return View(permit);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Permit model)
    {
      var p = await _context.Permits.FindAsync(model.PermitUID);
      if (p == null)
      {
        return NotFound();
      }

      p.AccessGroup = model.AccessGroup;
      p.PermitNumber = model.PermitNumber;
      p.PermitAmountDue = model.PermitAmountDue;
      p.DepositFee = model.DepositFee;
      p.DepositAmountPaid = model.DepositAmountPaid;
      p.EffectiveDate = model.EffectiveDate;
      p.ExpirationDate = model.ExpirationDate;
      p.IssueNumber = model.IssueNumber;
      p.IsDeactivated = model.IsDeactivated;
      p.IsMissing = model.IsMissing;
      p.IsTerminated = model.IsTerminated;
      p.ReplacementPermit = model.ReplacementPermit;
      p.WorkflowStatus = model.WorkflowStatus;
      p.Comment = model.Comment;

      p.ModifyDate = DateTime.Now;

      try
      {
        await _context.SaveChangesAsync().ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saving permit");
        ModelState.AddModelError("", ex.Message);
      }

      return View(p);
    }
    #endregion

    #region Create
    public async Task<IActionResult> Create(int customerId)
    {
      var permit = new Permit()
      {
        PurchasingCustomer = customerId,
      };
      return View(permit);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Permit model)
    {
      var permitId = 1;
      try
      {
        permitId = await _context.Permits.MaxAsync(x => x.PermitUID) + 1;
      }
      catch { }


      //model.AccessGroup = model.AccessGroup;
      //model.PermitNumber = model.PermitNumber;
      //model.PermitAmountDue = model.PermitAmountDue;
      //model.DepositFee = model.DepositFee;
      //model.DepositAmountPaid = model.DepositAmountPaid;
      //model.EffectiveDate = model.EffectiveDate;
      //model.ExpirationDate = model.ExpirationDate;
      //model.IssueNumber = model.IssueNumber;
      //model.IsDeactivated = model.IsDeactivated;
      //model.IsMissing = model.IsMissing;
      //model.IsTerminated = model.IsTerminated;
      //model.ReplacementPermit = model.ReplacementPermit;
      //model.WorkflowStatus = model.WorkflowStatus;
      //model.Comment = model.Comment;

      model.PermitUID = permitId;
      model.ModifyDate = DateTime.Now;
      _context.Permits.Add(model);

      try
      {
        await _context.SaveChangesAsync().ConfigureAwait(false);

        return RedirectToAction(nameof(Edit), new { id = permitId });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saving customer");
        ModelState.AddModelError("", ex.Message);
        return View(model);
      }
    }
    #endregion


  }
}
