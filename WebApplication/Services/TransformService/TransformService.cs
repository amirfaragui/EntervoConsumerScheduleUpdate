using ExcelDataReader;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using Entrvo.DAL;
using Entrvo.Models;
using Entrvo.Services.Models;

namespace Entrvo.Services
{
  public class TransformService : ITransformService
  {
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger<TransformService> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly string _dataFolder;
    private readonly string _archiveFolder;


    static char[] sepators = new char[] { ',', ';', '\t', '|', '#' };

    public TransformService(ApplicationDbContext context, IWebHostEnvironment hostEnvironment, ILogger<TransformService> logger)
    {
      _hostingEnvironment = hostEnvironment;
      _dbContext = context;
      _logger = logger;

      _dataFolder = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data");
      if (!Directory.Exists(_dataFolder))
      {
        Directory.CreateDirectory(_dataFolder);
      }
      _archiveFolder = Path.Combine(_dataFolder, "Archive");
      if (!Directory.Exists(_archiveFolder))
      {
        Directory.CreateDirectory(_archiveFolder);
      }
    }


    public async Task<int> GetTotalRecords(string fileName, bool firstLineHeader)
    {
      var path = Path.Combine(_dataFolder, fileName);
      if (File.Exists(path))
      {
        var ext = Path.GetExtension(fileName);
        if (ext == ".txt" || ext == ".csv")
        {
          using (var reader = new StreamReader(path))
          {
            var count = 0;
            var line = await reader.ReadLineAsync();
            while (line != null)
            {
              if (!string.IsNullOrWhiteSpace(line)) count++;
              line = await reader.ReadLineAsync();
            }
            if (count > 1)
            {
              if (firstLineHeader) count--;
            }
            return count;
          }
        }
        else if (ext.Contains(".xls"))
        {
          using var reader = ExcelReaderFactory.CreateReader(File.OpenRead(path), new ExcelReaderConfiguration()
          {
            FallbackEncoding = Encoding.GetEncoding(1252),
            LeaveOpen = false,
            AutodetectSeparators = sepators,
          });

          var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
          {
            ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
            {
              UseHeaderRow = firstLineHeader,
            }
          });

          return dataSet.Tables[0].Rows.Count;
        }
      }

      return 0;
    }

    public async Task ParseRecords(BatchTransformJobDescriptor jobDescriptor, CancellationToken cancellationToken = default)
    {
      if (jobDescriptor == null) throw new ArgumentNullException(nameof(jobDescriptor));

      Channel<JobProgress> channel = null;
      if (jobDescriptor.SendsFeedback)
      {
        channel = jobDescriptor.CreateChannel();
      }

      var jobs = jobDescriptor.Items.OrderBy(i => i.TimeStamp).ToList();
      foreach (var job in jobs)
      {
        var path = Path.Combine(_dataFolder, job.FileName);
        if (File.Exists(path))
        {
          _logger.LogInformation($"Start to parse file '{job.FileName}'.");

          var @event = new Event()
          {
            Type = JobType.ParseFile,
            Message = $"Start to parse file '{job.FileName}'."
          };

          var sw = new Stopwatch();
          sw.Start();


          int version = await CreateBatch(job.FileName);


          int count = 0, succeed = 0, failed = 0;

          await foreach (var line in ReadAllLines(path, job.FirstLineHeader))
          {
            count++;
            var processed = false;

            //TODO: parse 2 column file

            CardStatus status = CardStatus.NoChange;
            try
            {
              if (TryParseLine(line, job.Delimiter, out var card) && card != null)
              {
                status = await AddOrUpdateRecord(card, version, line, job.FileName);
                switch (status)
                {
                  case CardStatus.Created:
                  case CardStatus.Modified:
                  case CardStatus.NoChange:
                    succeed++;
                    processed = true;
                    break;

                  default:
                    failed++;
                    break;
                }
              }
              else
              {
                failed++;
              }
            }
            catch (Exception ex)
            {
              failed++;
              _logger.LogError(ex.ToString());
            }


            var progress = new JobProgress
            {
              TotalRecords = job.TotalRecords.Value,
              TotalProcessed = succeed + failed,
              Succeed = succeed,
              Failed = failed,
              Record = line,
              FileName = job.FileName,
              Status = status.ToString().ToLower(),
            };

            _logger.LogDebug(progress.ToString());

            if (channel != null)
            {
              await channel.Writer.WriteAsync(progress);
            }

            try
            {
              await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
              _logger.LogError(ex.ToString());
              _dbContext.ChangeTracker.Clear();
            }
          }

          var ext = Path.GetExtension(job.FileName);
          var archive = Path.Combine(_archiveFolder, string.Format("{0}{1}", DateTime.Now.ToString("yyyyMMddHHmmss"), ext));

          try
          {
            File.Move(path, archive);
          }
          catch (Exception ex)
          {
            _logger.LogError(ex.ToString());
          }

          sw.Stop();
          _logger.LogInformation($"File '{job.FileName}' parsed in {sw.Elapsed}.");

          @event.Details = $"Total {count} record processed, success: {succeed}, failed: {failed}, took {sw.Elapsed}";
          @event.FileUrl = archive;

          _dbContext.Events.Add(@event);
          await _dbContext.SaveChangesAsync(cancellationToken);
        }
      }

      jobDescriptor.Status = JobStatus.Completed;

    }

    private async IAsyncEnumerable<string> ReadAllLines(string fileName, bool skipHeaderLine)
    {
      var ext = Path.GetExtension(fileName);
      if (ext == ".txt" || ext == ".csv")
      {
        using (var reader = new StreamReader(fileName))
        {
          var line = await reader.ReadLineAsync();
          if (line != null && skipHeaderLine)
          {
            line = await reader.ReadLineAsync();
          }

          while (line != null)
          {
            yield return line;

            line = await reader.ReadLineAsync();
          }
        }
      }
      else if (ext.Contains(".xls"))
      {
        using var reader = ExcelReaderFactory.CreateReader(File.OpenRead(fileName), new ExcelReaderConfiguration()
        {
          FallbackEncoding = Encoding.GetEncoding(1252),
          LeaveOpen = false,
          AutodetectSeparators = sepators,
        });

        var dataSet = reader.AsDataSet(new ExcelDataSetConfiguration()
        {
          ConfigureDataTable = (tableReader) => new ExcelDataTableConfiguration()
          {
            UseHeaderRow = skipHeaderLine,
          }
        });

        foreach (DataRow row in dataSet.Tables[0].Rows)
        {
          var text = string.Join(",", row.ItemArray);
          yield return text;
        }
      }
    }

    private bool TryParseLine(string line, string delimiter, out Card? card)
    {
      card = null;
      var fields = line.Split(sepators);
      if (fields.Length != 9) return false;

      card = new Card()
      {
        ClientRef = fields[0],
      };

      if (decimal.TryParse(fields[7], out var value))
      {
        card.Amount = value;
      }

      return true;
    }

    private async Task<CardStatus> AddOrUpdateRecord(Card model, int version, string line, string source)
    {
      if (string.IsNullOrEmpty(model.ClientRef)) return CardStatus.Error;

      var dbItem = await _dbContext.Cards.Where(i => i.ClientRef == model.ClientRef).OrderByDescending(i => i.Version).FirstOrDefaultAsync();
      if (dbItem == null)
      {
        dbItem = new Card()
        {
          ClientRef = model.ClientRef,
          Amount = model.Amount,
          ValidUntil = model.ValidUntil,

          Version = version,
          TimeModified = DateTime.Now,
          Status = CardStatus.Created
        };
        try
        {
          var duplicate = _dbContext.ChangeTracker.Entries<Card>().Where(i => i.Entity.ClientRef == model.ClientRef && i.State == EntityState.Added).FirstOrDefault();
          if (duplicate == null)
          {
            _dbContext.Cards.Add(dbItem);

            return CardStatus.Created;
          }
          else
          {
            _logger.LogWarning($"Card number '{model.ClientRef}' is duplicated.");
            return CardStatus.Duplicated;
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.Message);
          return CardStatus.Error;
        }
      }
      else
      {
        dbItem.Version = version;

        dbItem.Amount = model.Amount;
        dbItem.TimeModified = DateTime.Now;
        if (dbItem.Status != CardStatus.Created) dbItem.Status = CardStatus.Modified;

        //var evt = new Event()
        //{
        //  Time = DateTime.Now,
        //  Type = EventType.Updated,
        //  Message = "Modified",
        //  CardNumber = model.ClientRef,
        //  Details = line,
        //  Source = source,
        //};
        //_dbContext.Events.Add(evt);

        return CardStatus.Modified;
      }
    }

    private async Task<int> CreateBatch(string fileName, DateTime? endValidity = null)
    {
      if (endValidity == null)
      {
        var today = DateTime.Today;
        endValidity = today.NextMonthEnd();
      }
      var batch = new Batch() { FileName = fileName, Time = DateTime.Now, NewEndValidityDate = endValidity.Value };
      _dbContext.Batchs.Add(batch);
      await _dbContext.SaveChangesAsync();
      return batch.Id;
    }

    public void Dispose()
    {
      //_dbContext.Dispose();
    }

  }
}
