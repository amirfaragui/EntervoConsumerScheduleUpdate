using Entrvo.Services.Models;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.RegularExpressions;

namespace Entrvo.Services
{
  public class FilesWatcherService : IFileWatcherService, IHostedService
  {

    #region config

    static int initialTimerInterval = 5;
    static int permittedIntervalBetweenFiles = 60;

    static bool LogFSWEvents = false;
    static bool LogFileReadyEvents = true;
    static bool FSWUseRegex = false;

    static string FSWRegex = null;

    #endregion

    #region private vars

    private List<string> _filteredFileTypes;
    private FileSystemWatcher _watcher;
    private readonly ILogger<FilesWatcherService> _logger;
    private readonly ISettingsService _settings;
    private readonly IWebHostEnvironment _environment;
    private string _baseDirectory;
    private bool _includeSubdirectories;
    #endregion


    public FilesWatcherService(IFilesWatchOptions options,
                               ISettingsService settings,
                               ILogger<FilesWatcherService> logger,
                               IWebHostEnvironment environment)
    {
      if (environment == null) throw new ArgumentNullException(nameof(environment));
      if (options == null) throw new ArgumentNullException(nameof(options));

      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      _settings = settings ?? throw new ArgumentNullException(nameof(settings));
      _environment = environment ?? throw new ArgumentNullException(nameof(environment));

      _filteredFileTypes = [.. options.FileTypes.Select(i => i.Replace("*", string.Empty))];
      _baseDirectory = options.BaseDirectory;
      _includeSubdirectories = options.IncludeSubdirectories;
    }

    #region events

    public event EventHandler<FileChangeEventArgs> OnFileReady;
    public event EventHandler<string> OnNewMessage;

    #endregion


    private readonly ConcurrentDictionary<string, FileChangeItem> filesEvents = new ConcurrentDictionary<string, FileChangeItem>();



    private void Watch(string FSWSource)
    {
      // If a directory is not specified, exit program.
      if (!(FSWSource.Length > 0) || _filteredFileTypes == null)
      {
        _logger.LogError("Cannot Proceed without FSWSource || fileTypes");
        return;
      }


      // Create a new FileSystemWatcher and set its properties.
      _watcher = new FileSystemWatcher();

      //watcher.Path = folder;
      _watcher.EnableRaisingEvents = false;
      _watcher.IncludeSubdirectories = _includeSubdirectories;
      _watcher.InternalBufferSize = 32768; //32KB

      _watcher.Path = FSWSource;

      _logger.LogInformation($"Watching Folder: {_watcher.Path}");


      // Watch for changes in LastAccess and LastWrite times, and
      // the renaming of files or directories.

      _watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.CreationTime |
                                              NotifyFilters.LastWrite | NotifyFilters.LastAccess;


      _watcher.Filter = "*.*";

      // Add event handlers.
      _watcher.Changed += OnFileChanged;
      _watcher.Created += OnFileChanged;

      // Begin watching.
      _watcher.EnableRaisingEvents = true;
      _logger.LogInformation("FileSystemWatcher Ready.");

    }


    private void OnFileChanged(object source, FileSystemEventArgs e)
    {

      if (FSWUseRegex && !Regex.IsMatch(e.Name, FSWRegex))
        return;

      try
      {

        FileInfo f = new FileInfo(e.FullPath);

        //discard event to block other file extentions...

        if (_filteredFileTypes.Any(str => f.Extension.Equals(str)))
        {
          DateTime eventTime = DateTime.Now;
          string fileName = e.Name;

          if (LogFSWEvents)
          {
            _logger.LogInformation($"Time: {eventTime.TimeOfDay}\t ChangeType: {e.ChangeType,-14} FileName: {fileName,-50} Path: {e.FullPath} ");
          }

          if (filesEvents.TryGetValue(fileName, out FileChangeItem r2NetWatchItem))
          {
            // in update process
            if (r2NetWatchItem.State == FileChangeItem.WatchItemState.Updating)
            {
              r2NetWatchItem.ResetTimer(e, eventTime);
            }
            // new / already reported file ready.
            else if (r2NetWatchItem.State == FileChangeItem.WatchItemState.Idle)
            {
              if (!r2NetWatchItem.WaitingForNextFile(eventTime))
              {
                // increase timer
                r2NetWatchItem.UpdateTimeForFileToBeReady(eventTime); //reset + interval
              }
              else
              {
                _logger.LogInformation($"FileName: {fileName} restarting count again.");
                r2NetWatchItem.ResetTimer(e, eventTime);
              }
            }
          }
          else // new supplier file
          {
            var watchItem = new FileChangeItem(e, initialTimerInterval);

            watchItem.OnFileReadyEvent += WatchItem_OnFileReady;
            watchItem.OnNewMessageEvent += WatchItem_OnNewMessage;
            watchItem.permittedIntervalBetweenFiles = permittedIntervalBetweenFiles;
            filesEvents.TryAdd(watchItem.FileName, watchItem);
          }
        }

      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }
    }


    private void WatchItem_OnNewMessage(object sender, string msg)
    {
      OnNewMessage?.Invoke(this, msg);
    }

    private void WatchItem_OnFileReady(object sender, FileChangeEventArgs e)
    {
      if (LogFileReadyEvents)
      {
        _logger.LogInformation($"File: {e.FileName,-50} ready.");
      }
      OnFileReady?.Invoke(this, e);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      var settings = await _settings.LoadSettingsAsync();
      string path = null;
      if (!string.IsNullOrEmpty(settings.DataFolder.MonitoringFolder))
      {
        path = settings.DataFolder.MonitoringFolder;
      }
      else
      {
        path = _baseDirectory;
      }

      if (path.Contains(':'))
      {
        _baseDirectory = path;
      }
      else
      {
        _baseDirectory = Path.Combine(_environment.WebRootPath, path);
      }

      StartWatcher();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      StopWatcher();
      return Task.CompletedTask;
    }

    private bool StartWatcher()
    {
      if (_watcher == null)
      {
        Watch(_baseDirectory);
        return true;
      }
      return false;
    }

    private void StopWatcher()
    {
      try { _watcher?.Dispose(); }
      catch (ObjectDisposedException) { }
      finally { _watcher = null; }

    }

    public void ChangeMonitoringFolder(string newFolder)
    {
      StopWatcher();
      _baseDirectory = newFolder;
      StartWatcher();
    }
  }
}