#nullable disable

using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Globalization;
using T2Importer.DAL;

namespace T2WebApplication.Services
{
  public class ConsumerService : IConsumerService
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConsumerService> _logger;
    private readonly IApiClient _client;
    private readonly ISettingsService _settingsService;
    private readonly IEmailService _emailService;

    private readonly string _dataFolder;
    private readonly string _reportFolder;

    public ConsumerService(ApplicationDbContext context,
                           IWebHostEnvironment hostingEnvironment,
                           ILogger<ConsumerService> logger,
                           IApiClient client,
                           IEmailService emailService,
                           ISettingsService settingsService)
    {
      _context = context;
      _logger = logger;
      _client = client;
      _settingsService = settingsService;
      _emailService = emailService;

      _dataFolder = Path.Combine(hostingEnvironment.WebRootPath, "App_Data");
      if (!Directory.Exists(_dataFolder))
      {
        Directory.CreateDirectory(_dataFolder);
      }
      _reportFolder = Path.Combine(_dataFolder, "ExportFiles");
      if (!Directory.Exists(_reportFolder))
      {
        Directory.CreateDirectory(_reportFolder);
      }
    }

    #region Download
    public async Task<bool> DownloadConsumersAsync(CancellationToken token = default)
    {
      var @event = new Event()
      {
        Type = JobType.ConsumerDownload,
        Message = "Start to download consumers from web API."
      };

      try
      {
        _logger.LogInformation("Clear Consumer database...");
        var records = await _context.Database.ExecuteSqlRawAsync("DELETE FROM Consumers");
        _logger.LogInformation($"Total {records} record(s) deleted.");


        var settings = await _settingsService.LoadSettingsAsync();
        await _client.Initialize();

        var count = 0;
        await foreach (var c in _client.GetConsumerDetailsAsync(settings.Destination.ContractNumber, token))
        {
          if (string.IsNullOrWhiteSpace(c.Person.MatchCode) || c.Person.MatchCode == "Non-Payroll") continue;

          var consumer = new T2Importer.DAL.Consumer()
          {
            LPN1 = c.Lpn1?.ToUpper(),
            LPN2 = c.Lpn2?.ToUpper(),
            LPN3 = c.Lpn3?.ToUpper(),
            ContractId = c.Consumer?.ContractId,
            ConsumerId = c.Consumer?.Id,
            FirstName = c.FirstName?.Replace("First", string.Empty).Trim(),
            LastName = c.Surname?.Replace("Last", string.Empty).Trim(),
            CardNumber = c.Identification?.CardNumber,
            ClientRef = c.Person.MatchCode,
            Memo1 = c.UserField1,
            Memo2 = c.UserField2,
            Memo3 = c.UserField3,
            Amount = Convert.ToDecimal(c.ConsumerAttributes.FlatRateAmt) / 100,
          };

          if (DateTime.TryParseExact(c.Identification.ValidUntil, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var validity))
          {
            consumer.ValidUntil = validity;
          }


          consumer.ParseNames();
          _logger.LogInformation($"Add consumer [{c}].");

          _context.Consumers.Add(consumer);
          count++;
        }

        await _context.SaveChangesAsync();

        var message = $"Total {count} consumer(s) downloaded.";
        _logger.LogInformation(message);
        @event.Details = message;

        await _settingsService.SetConsumerDatabaseInitialized(true);

        return true;

      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        @event.Details = ex.Message;
        throw;
      }
      finally
      {
        _context.Events.Add(@event);
        await _context.SaveChangesAsync();
      }
    }
    #endregion

    #region Update
    public async Task<bool> UpdateConsumersAsync(CancellationToken cancellation = default)
    {
      var @event = new Event()
      {
        Type = JobType.ConsumerUpdate,
        Message = "Start to update consumers via web API."
      };

      try
      {
        var settings = await _settingsService.LoadSettingsAsync();
        var url = settings.Destination.Server.Replace("https", "http");

        await _client.Initialize();

        var sw = new Stopwatch();
        _logger.LogInformation("Start to push consumers to server...");
        sw.Start();

        var consumers = from u in _context.Consumers
                        where u.IsActive == true
                        join c in _context.Cards on u.ClientRef equals c.ClientRef into grouping
                        from p in grouping.DefaultIfEmpty()
                        select new { u, p };

        var list = await consumers.Where(i => i.p == null || i.p.Amount != i.u.Amount).ToListAsync();

        _logger.LogInformation($"Total {list.Count} consumers need to be updated.");

        var expDate = DateTime.Today.CurrentMonthEnd();
        var count = 0;

        foreach (var item in list)
        {
          //if (item.u.ClientRef != "229931") continue;

          var c = await _client.GetConsumerAsync(item.u.ContractId, item.u.ConsumerId);

          if (item.p == null)
          {
            c.Identification.ValidUntil = expDate.ToString("yyyy-MM-dd");
          }
          else
          {
            c.ConsumerAttributes.FlatRateAmt = (int)item.p.Amount;
          }
          try
          {
            var result = await _client.UpdateConsumerAsync(c);
            _logger.LogInformation($"[SUCCESS ] Updating consumer '{c.Person.FirstName} {c.Person.Surname}', reference#: {c.Person.MatchCode}, href: {c.Consumer.Href}.");
            count++;

            var record = await _context.Consumers.FirstOrDefaultAsync(i => i.Id == item.u.Id);
            if (record != null)
            {
              if (item.p == null)
              {
                record.IsActive = false;
              }
              else
              {
                record.Amount = c.ConsumerAttributes.FlatRateAmt;
              }
            }

            await _context.SaveChangesAsync();
          }
          catch (Exception ex)
          {
            _logger.LogError(ex.ToString());
            _logger.LogInformation($"[ERROR   ] Updating consumer '{c.Person.FirstName} {c.Person.Surname}', reference#: {c.Person.MatchCode}, href: {c.Consumer.Href}.");
          }
        }

        sw.Stop();
        var message = $"{count} consumers have been updated in {sw.Elapsed}.";
        _logger.LogInformation(message);
        @event.Details = message;

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        @event.Details = ex.Message;
        return false;
      }
      finally
      {
        _context.Events.Add(@event);
        await _context.SaveChangesAsync();
      }
    }
    #endregion

    #region Export files

    public async Task<bool> GenerateO1ExportFile(CancellationToken cancellation = default)
    {
      var fileName = string.Format("Payroll Export {0}.txt", DateTime.Now.ToString("yyyyMMdd"));

      var @event = new Event()
      {
        Type = JobType.Generate01Report,
        Message = $"Generate Payroll Export file '{fileName}'."
      };

      _logger.LogInformation($"Start to generate {fileName} file");
      var path = Path.Combine(_reportFolder, fileName);

      var batch = new Export01Batch() { Time = DateTime.Now, FileName = fileName };
      _context.Export01Batchs.Add(batch);
      await _context.SaveChangesAsync();

      int batchNo = batch.Id;
      var date = DateTime.Today.CurrentMonthEnd().AddDays(1).ToString("yyyy-MM-dd h:mm");

      var count = 0;

      using (var writer = new StreamWriter(File.Create(path)))
      {
        var headers = new string[]
        {
        "BCH_NO",
        "INVSPL_CD",
        "INDVROLETP_CD",
        "TRANS_CD",
        "CORPINDV_CD",
        "DEDTRANS_NO",
        "DT",
        "ISSUE_PRCAL_PAY_NO",
        "FR_PRCAL_PAY_NO",
        "TO_PARCAL_PAY_NO",
        "AMT",
        "CHQTP_CD",
        "PAY_REVS_IND",
        "ORIG_TRANS_CD",
        "GL_CD",
        "MISACCT_CD",
        "ORG_CD",
        "THPTY_CD",
        "BONDANNCAMP_EFFYR",
        "INDV_ID"
        };
        await writer.WriteLineAsync(string.Join("\t", headers));


        var consumers = await _context.Consumers.OrderBy(i => i.ClientRef).ToArrayAsync();

        foreach (var r in consumers)
        {
          var data = new string[]
          {
          batchNo.ToString(),
          string.Empty,
          "E",
          r.Memo2?.Substring(0,4) ?? string.Empty,
          "I",
          (++count).ToString(),
          date,
          string.Empty,
          string.Empty,
          string.Empty,
          r.Amount?.ToString("F0") ?? "0",
          "1",
          "0",
          r.Memo2?.Substring(0,4) ?? string.Empty,
          string.Empty,
          string.Empty,
          string.Empty,
          string.Empty,
          string.Empty,
          r.ClientRef
          };

          await writer.WriteLineAsync(string.Join("\t", data));

        }
      }

      var message = $"{fileName} file created, total {count} records.";
      _logger.LogInformation(message);
      @event.Details = message;
      @event.FileUrl = path;
      _context.Events.Add(@event);

      var emailEvent = new Event()
      {
        Type = JobType.SendEmail,
        Message = $"Sending email with attachment '{fileName}'.",
      };
      _context.Events.Add(emailEvent);

      try
      {
        _logger.LogInformation($"Sending email with attachment {fileName}...");
        await _emailService.SendEmailAsync(fileName, $"Here's the payroll export file {fileName}.", path);
        emailEvent.Details = "Done";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        emailEvent.Details = ex.Message;
      }

      await _context.SaveChangesAsync();

      return true;
    }

    public async Task<bool> GenerateO4ExportFile(CancellationToken cancellation = default)
    {
      var fileName = string.Format("Payroll Deduction {0}.txt", DateTime.Now.ToString("yyyyMMdd"));

      var @event = new Event()
      {
        Type = JobType.Generate04Report,
        Message = $"Generate Payroll Deduction file '{fileName}'."
      };

      _logger.LogInformation($"Start to generate {fileName} file");
      var path = Path.Combine(_reportFolder, fileName);

      var batch = new Export04Batch() { Time = DateTime.Now, FileName = fileName };
      _context.Export04Batchs.Add(batch);
      await _context.SaveChangesAsync();

      int batchNo = batch.Id;
      var date = DateTime.Today.ToString("dd-MMM-yy").ToUpper().Replace(".", string.Empty);

      var count = 0;
   
      using (var writer = new StreamWriter(File.Create(path)))
      {
        var headers = new string[]
        {
        "INDV_ID",
        "SRNAME",
        "FRNAME",
        "GTNCTL_RUN_NO",
        "DT",
        "TRANS_CD",
        "CORPINDV_CD",
        "AMT",
        "CR_CORP_GL_CD"
        };
        await writer.WriteLineAsync(string.Join("\t", headers));


        var consumers = await _context.Consumers.OrderBy(i => i.ClientRef).ToArrayAsync();

        foreach (var r in consumers)
        {
          var data = new string[]
          {
          r.ClientRef,
          r.LastName,
          r.FirstName,
          batchNo.ToString(),
          date,
          r.Memo2?.Substring(0,4) ?? string.Empty,
          "I",
          r.Amount?.ToString("F0") ?? "0",
          "011501414990501000"
          };

          await writer.WriteLineAsync(string.Join("\t", data));
          count++;
        }
      }

      var message = $"{fileName} file created, total {count} records.";
      _logger.LogInformation(message);
      @event.Details = message;
      @event.FileUrl = path;

      _context.Events.Add(@event);

      var emailEvent = new Event()
      {
        Type = JobType.SendEmail,
        Message = $"Sending email with attachment '{fileName}'.",
      };
      _context.Events.Add(emailEvent);

      try
      {
        _logger.LogInformation($"Sending email with attachment {fileName}...");
        await _emailService.SendEmailAsync(fileName, $"Here's the payroll export file {fileName}.", path);
        emailEvent.Details = "Done";
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        emailEvent.Details = ex.Message;
      }

      await _context.SaveChangesAsync();

      return true;

    }

    #endregion
  }
}
