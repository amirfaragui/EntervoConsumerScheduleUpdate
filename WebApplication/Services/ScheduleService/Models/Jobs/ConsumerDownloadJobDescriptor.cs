using System.Threading.Channels;
using T2WebApplication.Models;

namespace T2WebApplication.Services.Models
{
  public class ConsumerDownloadJobDescriptor : ScheduleJobDescriptor, IJobDescriptor, IJobFeedback, IDisposable
  {
    private readonly IConsumerService _comsumerService;

    private Channel<JobProgress> _channel;

    public ConsumerDownloadJobDescriptor(IConsumerService consumerService)
    {
      _comsumerService = consumerService;
    }

    public bool SendsFeedback { get; set; }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
      return StartAsync(_comsumerService.DownloadConsumersAsync(cancellationToken), cancellationToken);
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
