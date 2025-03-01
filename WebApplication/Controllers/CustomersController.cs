using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using T2Importer.DAL;
using T2WebApplication.Services;

namespace T2WebApplication.Controllers
{
  public class CustomersController : EntityController<Customer>
  {
    private readonly ApplicationDbContext _context;


    public CustomersController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<CustomersController> logger, IImportingService importService): base(hostingEnvironment, logger, importService)
    {
      _context = context;
    }

    #region List
    public IActionResult Index()
    {
      return View();
    }

    public async Task<IActionResult> _Read([DataSourceRequest] DataSourceRequest request)
    {
      var cards = await _context.Customers.AsNoTracking().ToDataSourceResultAsync(request);
      return Json(cards);
    }

    public async Task<IActionResult> _Destroy([DataSourceRequest] DataSourceRequest request, Customer customer)
    {
      var dbItem = await _context.Customers.FindAsync(customer.CustomerUID);
      if (dbItem != null)
      {
        _context.Customers.Remove(dbItem);
        await _context.SaveChangesAsync();
        return Json(dbItem);
      }
      return NotFound();
    }
    #endregion

    #region Edit
    public async Task<IActionResult> Edit(int id)
    {
      var customer = await _context.Customers.FindAsync(id);
      if (customer == null)
      {
        return NotFound();
      }
      return View(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Customer model)
    {
      var customer = await _context.Customers.FindAsync(model.CustomerUID);
      if (customer == null)
      {
        return NotFound();
      }

      customer.BalanceDue = model.BalanceDue;
      customer.AccountBalance = model.AccountBalance;
      customer.FirstName = model.FirstName;
      customer.LastName = model.LastName;
      customer.MiddleName = model.MiddleName;
      customer.NamePrefix = model.NamePrefix;
      customer.NameSuffix = model.NameSuffix;
      customer.GroupName = model.GroupName;
      customer.AllotmentGroup = model.AllotmentGroup;
      customer.Classification = model.Classification;
      customer.Subclassification = model.Subclassification;
      customer.HomePhone = model.HomePhone;
      customer.WorkPhone = model.WorkPhone;
      customer.OtherPhone = model.OtherPhone;
      customer.NonEmployeeID = model.NonEmployeeID;
      customer.ModifyDate = DateTime.Now;

      try
      {
        await _context.SaveChangesAsync().ConfigureAwait(false);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saving customer");
        ModelState.AddModelError("", ex.Message);
      }

      return View(customer);
    }
    #endregion

    #region Create
    public async Task<IActionResult> Create()
    {
      var customer = new Customer();
      return View(customer);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Customer customer)
    {
      var customerId = 1;
      try
      {
        customerId = await _context.Customers.MaxAsync(x => x.CustomerUID) + 1;
      }
      catch { }
      
      customer.CustomerUID = customerId;
      customer.ModifyDate = DateTime.Now;
      _context.Customers.Add(customer);

      try
      {
        await _context.SaveChangesAsync().ConfigureAwait(false);

        return RedirectToAction(nameof(Edit), new { id = customerId });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error saving customer");
        ModelState.AddModelError("", ex.Message);
        return View(customer);
      }
    }

    #endregion

  }
}
