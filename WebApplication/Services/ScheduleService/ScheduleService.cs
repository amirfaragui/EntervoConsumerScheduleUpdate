using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Entrvo.Models;
using Entrvo.Services.Models;

namespace Entrvo.Services
{
  public class ScheduleService : IHostedService, IScheduleService
  {
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly ILogger<ScheduleService> _logger;
    private readonly IServiceScopeFactory _scopedServiceFactory;
    private readonly ISettingsService _settingsService;
    private readonly ScheduleOptions _scheduleOptions;

    private readonly ConcurrentQueue<IJobDescriptor> _jobs;

    private CancellationTokenSource _cancellationTokenSource;
    private IServiceScope _scope;

    public ScheduleService(IWebHostEnvironment hostingEnvironment,
                           IServiceScopeFactory scopedServiceFactory,
                           ISettingsService settingsService,
                           IOptions<ScheduleOptions> options,
                           ILogger<ScheduleService> logger)
    {
      if (options == null) throw new ArgumentNullException(nameof(options));
      _hostingEnvironment = hostingEnvironment;
      _logger = logger;
      _scopedServiceFactory = scopedServiceFactory;
      _settingsService = settingsService;

      _jobs = new ConcurrentQueue<IJobDescriptor>();
      _scheduleOptions = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
      var setting = await _settingsService.LoadSettingsAsync();

      _scope = _scopedServiceFactory.CreateScope();
      _cancellationTokenSource = new CancellationTokenSource();

#pragma warning disable 4014
      Task.Factory.StartNew(() =>
      {
        try
        {
           ScheduleWorkProc(_cancellationTokenSource.Token);
          //_logger.LogInformation("Will not run the schedule service");
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
      }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

      Task.Factory.StartNew(() =>
      {
        try
        {
         QueueWorkProc(_cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
      }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
#pragma warning restore 4014
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
      if (_cancellationTokenSource != null)
      {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
      }

      _scope.Dispose();
      return Task.CompletedTask;
    }

    private async Task ScheduleWorkProc(CancellationToken token)
    {
      try
      {
        //var minute = DateTime.Now.Subtract(DateTime.Today).TotalMinutes;
        //await Task.Delay(TimeSpan.FromMinutes(1440 - minute), token);

        // run the job now then have a timer
        await Task.Delay(1000);
        await AddConsumersPushJob();

        var now = DateTime.Now.TimeOfDay.TotalMinutes;
        TimeSpan timeToUpload = TimeSpan.Parse(_scheduleOptions.DailyEntervoUpdateSchedule);
        var delay = (1440 + timeToUpload.TotalMinutes - now) % 1440;
        await Task.Delay(TimeSpan.FromMinutes(delay));
        while (true)
        {
          token.ThrowIfCancellationRequested();

      /**    var now = DateTime.Today;
          var dayUntilMonthEnd = (int)now.CurrentMonthEnd().Subtract(now).TotalDays;
          var hour = DateTime.Now.Hour;

          if (dayUntilMonthEnd == _scheduleOptions.DaysBeforeMonthEndFor1stReport)
          {
            switch (hour)
            {
              case 1:
                await AddConsumersDownloadJob();
                break;

              case 5:
                await AddExport04ReportJOb();
                break;
            }
          }
          if (dayUntilMonthEnd == _scheduleOptions.DaysBeforeMonthEndFor2ndReport)
          {
            switch (hour)
            {
              case 1:
                await AddPayrollFileDownloadJob();
                break;  **/

          //    case 3:
                await AddConsumersPushJob();
            //    break;

              //case 8:
              //  await AddExport01ReportJOb();
               // break;
           // }
         // }

          await Task.Delay(TimeSpan.FromDays(1), token);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }
    }

    private async Task QueueWorkProc(CancellationToken token)
    {
      try
      {
        while (true)
        {
          token.ThrowIfCancellationRequested();

          IJobDescriptor job = null;

          try
          {
            if (_jobs.TryPeek(out job))
            {
              if (job.Status == JobStatus.Created)
              {
                job.StartAsync(token);
              }

              await job.Completion;
            }
          }
          catch (Exception ex)
          {
            _logger.LogError(ex.ToString());
          }
          finally
          {
            if (job != null)
            {
              if (job is IDisposable disp)
              {
                disp.Dispose();
              }
              _jobs.TryDequeue(out job);
            }
          }

          await Task.Delay(1000, token);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }
    }


    public bool IsFileInUse(string fileName)
    {
      return _jobs.OfType<BatchTransformJobDescriptor>()
                  .Where(i => i.Status == JobStatus.Running)
                  .SelectMany(i => i.Items)
                  .Any(i => i.FileName == fileName);
    }

    public bool TryGetJob(Guid jobId, out IJobDescriptor? jobDescriptor)
    {
      jobDescriptor = _jobs.FirstOrDefault(i => i.JobId == jobId);
      return jobDescriptor != null;
    }

    public Task StartJob(Guid jobId, CancellationToken cancellationToken = default)
    {
      if (TryGetJob(jobId, out var job))
      {
        if (job.Status == JobStatus.Created)
        {
          job.Status = JobStatus.Running;

          var task = job.StartAsync(cancellationToken);

          return task;
        }
        else
        {
          if (job.Status == JobStatus.Running)
          {

          }
          return job.Completion;
        }
      }
      return Task.FromException(new Exception($"Job does not exist."));
    }

    public Task StartJob(IJobDescriptor job, CancellationToken cancellationToken = default)
    {
      if (job == null) throw new ArgumentNullException(nameof(job));

      if (job.Status == JobStatus.Created)
      {
        job.Status = JobStatus.Running;

        var task = job.StartAsync(cancellationToken);

        return task;
      }
      else
      {
        if (job.Status == JobStatus.Running)
        {

        }
        return job.Completion;
      }
    }

    public async Task<IJobDescriptor> AddPayrollFileDownloadJob()
    {
      var descriptor = _scope.ServiceProvider.GetRequiredService<FtpDownloadJobDescriptor>();
      _jobs.Enqueue(descriptor);
      return descriptor;
    }

    public async Task<IJobDescriptor> AddManualFileParsingJob(IFileSource job)
    {
      var jobDescriptor = await AddFilesParsingJob(job);
      if (jobDescriptor is IJobFeedback feedback)
      {
        feedback.SendsFeedback = true;
      }
      return jobDescriptor;
    }

    public async Task<IJobDescriptor> AddFilesParsingJob(params IFileSource[] files)
    {
      var descriptor = _scope.ServiceProvider.GetRequiredService<BatchTransformJobDescriptor>();
      foreach (var file in files)
      {
        var path = Path.Combine(_hostingEnvironment.WebRootPath, "App_Data", file.FileName);
        if (File.Exists(path))
        {
          var job = new BatchItem()
          {
            FileName = file.FileName,
            Delimiter = file.Delimiter,
            FirstLineHeader = file.FirstLineHeader,
            TimeStamp = file.TimeStamp,
          };

          await descriptor.AddItem(job);
        }
      }

      _jobs.Enqueue(descriptor);
      return descriptor;
    }

    public async Task<IJobDescriptor> AddConsumersDownloadJob()
    {
      var descriptor = _scope.ServiceProvider.GetRequiredService<ConsumerDownloadJobDescriptor>();
      _jobs.Enqueue(descriptor);
      return descriptor;
    }

    public async Task<IJobDescriptor> AddConsumersPushJob()
    {
      var descriptor = _scope.ServiceProvider.GetRequiredService<ConsumerUploadJobDescriptor>();
      _jobs.Enqueue(descriptor);
      return descriptor;
    }

    public async Task<IJobDescriptor> AddExport01ReportJOb()
    {
      var descriptor = _scope.ServiceProvider.GetRequiredService<Export01ReportJobDescriptor>();
      _jobs.Enqueue(descriptor);
      return descriptor;
    }

    public async Task<IJobDescriptor> AddExport04ReportJOb()
    {
      var descriptor = _scope.ServiceProvider.GetRequiredService<Export04ReportJobDescriptor>();
      _jobs.Enqueue(descriptor);
      return descriptor;
    }
  }
}
