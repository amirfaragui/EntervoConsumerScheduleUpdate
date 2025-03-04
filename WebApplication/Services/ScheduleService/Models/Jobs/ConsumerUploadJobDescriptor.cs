using System.Threading.Channels;
using Entrvo.Models;

namespace Entrvo.Services.Models
{
  public class ConsumerUploadJobDescriptor : ScheduleJobDescriptor, IJobDescriptor, IJobFeedback, IDisposable
  {
    private readonly IConsumerService _comsumerService;

    private Channel<JobProgress> _channel;

    public ConsumerUploadJobDescriptor(IConsumerService consumerService)
    {
      _comsumerService = consumerService;
    }

    public bool SendsFeedback { get; set; }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
      return StartAsync(_comsumerService.UpdateConsumersAsync(cancellationToken), cancellationToken);
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
