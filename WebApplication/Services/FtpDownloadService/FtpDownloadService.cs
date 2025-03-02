#nullable disable

using Renci.SshNet;
using Renci.SshNet.Async;
using Renci.SshNet.Sftp;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.Text.RegularExpressions;
using Entrvo.DAL;
using T2WebApplication.Models;
using T2WebApplication.Services.Models;

namespace T2WebApplication.Services
{
  //host = @"sftpflex.t2hosted.ca";
  //username = "UHNftp";
  //password = "watchdog2037";
  public class FtpDownloadService : IFtpDownloadService
  {
    private readonly ILogger<FtpDownloadService> _logger;
    private readonly ISettingsService _settingsService;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly IScheduleService _scheduleService;
    private readonly ApplicationDbContext _dbContext;


    public FtpDownloadService(ApplicationDbContext context,
                              ISettingsService settingsService,
                              IScheduleService scheduleService,
                              IWebHostEnvironment hostEnvironment,
                              ILogger<FtpDownloadService> logger)
    {
      _dbContext = context;
      _settingsService = settingsService;
      _scheduleService = scheduleService;
      _logger = logger;
      _hostingEnvironment = hostEnvironment;

      //_semaphore = new SemaphoreSlim(1);

      var rootPath = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data");
      if (!Directory.Exists(rootPath))
      {
        try
        {
          Directory.CreateDirectory(rootPath);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
      }
    }

    private DateTime? ExtractDateTime(string filename)
    {
      var numbers = Regex.Split(filename, @"\D+").FirstOrDefault(i => i.Length == 10);
      if (numbers == null) return null;
      if (DateTime.TryParseExact(numbers, "MMddyyHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var datetime)) return datetime;
      return null;
    }

    private async Task<bool> DownloadFileAsync(SftpClient client, SftpFile file)
    {
      var rootPath = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", file.Name);
      try
      {
        using var stream = File.OpenWrite(rootPath);
        await client.DownloadAsync(file.FullName, stream);
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        return false;
      }
    }

    public async Task<bool> StartDownloadAsync(CancellationToken token = default)
    {
      var @event = new Event()
      {
        Type = JobType.FtpDownload,
        Message = "Start to download files from FTP site"
      };
      _dbContext.Events.Add(@event);

      try
      {
        var settings = await _settingsService.LoadSettingsAsync();
        var source = settings.Source;


        Guid? batchId = null;

        if (!string.IsNullOrEmpty(source.Server) && !string.IsNullOrEmpty(source.UserName) && !string.IsNullOrEmpty(source.Password))
        {
          using (var sftp = new SftpClient(source.Server, source.UserName, source.Password))
          {
            try
            {
              _logger.LogDebug("Connecting to SFTP server...");
              sftp.Connect();

              var rootFolder = "/" + source.RootDirectory.Trim('/');
              var archiveFolder = rootFolder.TrimEnd('/') + "/Archive";

              if (!sftp.Exists(archiveFolder))
              {
                sftp.CreateDirectory(archiveFolder);
              }

              _logger.LogDebug("Retrieving file list...");
              var files = await sftp.ListDirectoryAsync(rootFolder);
              files = files.Where(i => i.IsDirectory == false && i.Attributes.Size > 0).ToArray();
              var message = $"Found {files.Count()} file(s) on the server.";
              _logger.LogDebug(message);
              @event.Details = message; 

              var fullList = files.Select(i => new
              {
                time = ExtractDateTime(i.Name),
                file = i
              }).OrderByDescending(i => i.time).ToArray();

              var filesToIgnore = new List<SftpFile>();

              var fullFile = fullList.FirstOrDefault();
              if (fullFile != null)
              {
                filesToIgnore.AddRange(fullList.Skip(1).Select(i => i.file).ToArray());
              }

              List<IFileSource> items = new List<IFileSource>();

              if (fullFile != null)
              {
                _logger.LogDebug($"Downloading '{fullFile.file.FullName}' file...");

                await DownloadFileAsync(sftp, fullFile.file);

                items.Add(new BatchItem()
                {
                  FileName = fullFile.file.Name,
                  TimeStamp = fullFile.time ?? DateTime.Now,
                  FirstLineHeader = true,
                  Delimiter = "\t",
                });

                var archiveFile = archiveFolder + "/" + fullFile.file.Name;
                fullFile.file.MoveTo(archiveFile);

                @event.Message = $"File '{fullFile.file.Name}' has been downloaded.";
              }

              var result = await _scheduleService.AddFilesParsingJob(items.ToArray());
              batchId = result.JobId;


              if (filesToIgnore.Any())
              {
                _logger.LogDebug($"Total of {filesToIgnore.Count()} file(s) will be ignored.");

                foreach (var file in filesToIgnore)
                {
                  _logger.LogDebug($"File '{file.FullName}' will be ignored.");

                  var archiveFile = archiveFolder + "/" + file.Name;
                  file.MoveTo(archiveFile);
                }
              }

              sftp.Disconnect();
            }
            catch (Exception ex)
            {
              _logger.LogError(ex.ToString());
            }
          }
        }

        return true;
      }
      catch (Exception ex)
      {
        @event.Message = ex.Message;
        _logger.LogError(ex.ToString());
        return false;
      }
      finally
      {
        await _dbContext.SaveChangesAsync();
      }
    }
  }
}
