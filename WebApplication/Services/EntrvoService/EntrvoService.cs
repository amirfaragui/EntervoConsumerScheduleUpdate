using Entrvo.DAL;
using Entrvo.Models;
using Entrvo.Services.Models;
using EntrvoWebApp.Services;
using EntrvoWebApp.Services.Models;
using Kendo.Mvc.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Reflection;

namespace Entrvo.Services
{
  public class EntrvoService : BackgroundService, IEntrvoService
  {
    private readonly ISettingsService _settingsService;
    private readonly ILogger<EntrvoService> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IFileWatcherService _fileWatcherService;
    private readonly IMqttService _mqttService;

    private readonly ConcurrentQueue<string> _fileQueue = new ConcurrentQueue<string>();

    public EntrvoService(ISettingsService settingsService,
                         IMqttService mqttService,
                         IServiceScopeFactory serviceScopeFactory,
                         IFileWatcherService fileWatcherService,
                         ILogger<EntrvoService> logger)
    {
      _settingsService = settingsService;
      _logger = logger;
      _serviceScopeFactory = serviceScopeFactory;
      _fileWatcherService = fileWatcherService;
      _mqttService = mqttService;

      _fileWatcherService.OnFileReady += OnFileReady;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
      while (!stoppingToken.IsCancellationRequested)
      {
        if (_fileQueue.TryDequeue(out var file))
        {
          _logger.LogInformation($"Processing file {file}");
          await ProcessFile(file, stoppingToken);
        }
        await Task.Delay(1000, stoppingToken);
      }
    }

    public Task Enqueue(string file)
    {
      _fileQueue.Enqueue(file);
      return Task.CompletedTask;
    }

    private void OnFileReady(object? sender, FileChangeEventArgs e)
    {
      _fileQueue.Enqueue(e.FullPath);
    }

    private async Task ProcessFile(string file, CancellationToken cancellationToken)
    {
      using var scope = _serviceScopeFactory.CreateScope();
      var parser = scope.ServiceProvider.GetRequiredService<IFileParser>();
      var importer = scope.ServiceProvider.GetRequiredService<IEntrvoConsumerService>();
      var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

      var settings = await _settingsService.LoadSettingsAsync();
      var backupFolder = settings.DataFolder.BackupFolder;
      var fileName = Path.GetFileNameWithoutExtension(file);
      var ext = Path.GetExtension(file);
      var backupFile = Path.Combine(backupFolder, $"{fileName}-{DateTime.Now:yyMMddHHmm}{ext}");


      var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

      var properties = typeof(EntrvoRecord).GetProperties(BindingFlags.Public | BindingFlags.Instance).ToArray();
      var mappings = properties.Select(i => new ColumnMappingModel()
      {
        Target = i.Name,
        Header = i.Name,
        Value = i.PropertyType.ToString(),
      }).ToArray();
      for (var i = 0; i < mappings.Length; i++)
      {
        mappings[i].Index = i + 1;
      }

      var endValidity = mappings.FirstOrDefault(i => i.Name == "EndValidity");
      if (endValidity != null)
      {
        endValidity.Format = "yyyyMMdd";
      }

      var unlimitedEndDate = new DateTime(1900, 1, 1);

      var model = new ImportSourceModel
      {
        FileName = file,
        KeySelector = s => s[1],
        TargetType = typeof(EntrvoRecord),
        Mappings = mappings
      };

      await foreach (var data in parser.ParseFileAsync<EntrvoRecord>(model, cts.Token))
      {
        _logger.LogInformation($"Parsed {data}");
        try
        {
          if (data.StartValidity == unlimitedEndDate) data.StartValidity = null;
          if (data.EndValidity == unlimitedEndDate) data.EndValidity = null;

          var cardNumber = data.GetId();
          _logger.LogInformation($"File processing => Parsed card number {cardNumber}");
          var dbItem = await context.Consumers.FirstOrDefaultAsync(i => i.CardNumber == cardNumber);
          Api.Models.ConsumerDetails? consumer = null;
          if (dbItem?.ConsumerId != null)
          {
            _logger.LogInformation($"File processing => exisitng consumer id {dbItem?.ConsumerId}");
            consumer = await importer.UpdateConsumerAsync(dbItem.ContractId ?? settings.Destination.ContractNumber.ToString(),
                                                          dbItem.ConsumerId!,
                                                          data,
                                                          cts.Token);
          }
          else
          {
            _logger.LogInformation($"File processing => not found consumer [{cardNumber}] on local DB");
            consumer = await importer.UpdateConsumerAsync(data, cts.Token);
          }
          if (consumer != null)
          {
            try
            {
              if (dbItem == null)
              {
                dbItem = new Consumer();
                context.Consumers.Add(dbItem);
              }

              dbItem.CardNumber = cardNumber;
              dbItem.LPN1 = data.LPN1;
              dbItem.LPN2 = data.LPN2;
              dbItem.LPN3 = consumer.Lpn3;
              dbItem.FirstName = data.FirstName;
              dbItem.LastName = data.LastName;
              dbItem.ConsumerId = consumer.Consumer.Id;
              dbItem.ContractId = consumer.Consumer.ContractId;
              dbItem.Memo1 = consumer.Memo;
              dbItem.Memo2 = consumer.UserField1;
              dbItem.Memo3 = consumer.UserField2;
              if (DateTime.TryParse(consumer.Consumer.ValidUntil, out var validUntil))
              {
                dbItem.ValidUntil = validUntil;
              }

              var evt = new Event()
              {
                Time = DateTime.Now,
                Message = $"Consumer {dbItem.CardNumber} updated from file '{Path.GetFileName(file)}'.",
                Details = data.ToString(),
                Type = JobType.ConsumerUpdate,
                ConsumerId = dbItem.Id,
                FileUrl = backupFile,
              };
              context.Events.Add(evt);

              await context.SaveChangesAsync(cts.Token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
              _logger.LogError(ex.ToString());
            }
            finally
            {
              context.ChangeTracker.Clear();
            }
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
      }

      #region Backup file
      if (!string.IsNullOrEmpty(backupFolder) && Directory.Exists(backupFolder))
      {
        if (File.Exists(backupFile))
        {
          File.Delete(backupFile);
        }

        try
        {
          File.Move(file, backupFile);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
      }
      #endregion
    }
  }
}
