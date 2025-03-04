using System.Threading.Channels;
using Entrvo.Models;

namespace Entrvo.Services.Models
{
  public class FtpDownloadJobDescriptor : ScheduleJobDescriptor, IJobDescriptor, IJobFeedback, IDisposable
  {
    private readonly IFtpDownloadService _downloadService;

    private Channel<JobProgress> _channel;

    public FtpDownloadJobDescriptor(IFtpDownloadService consumerService)
    {
      _downloadService = consumerService;
    }

    public bool SendsFeedback { get; set; }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
      return StartAsync(_downloadService.StartDownloadAsync(cancellationToken), cancellationToken);
    }


    public Channel<JobProgress> CreateChannel()
    {
      if (SendsFeedback &&_channel == null)
      {
        _channel = Channel.CreateUnbounded<JobProgress>();
      }
      return _channel;
    }

    public void Dispose()
    {
      if (_channel != null)
      {
        _channel.Writer.TryComplete();
      }
    }
  }
}
