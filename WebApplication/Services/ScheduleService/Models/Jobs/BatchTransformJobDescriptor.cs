using System.Threading.Channels;
using T2WebApplication.Models;

namespace T2WebApplication.Services.Models
{
  public class BatchTransformJobDescriptor : ScheduleJobDescriptor, IJobDescriptor, IJobFeedback, IDisposable
  {
    private readonly ITransformService _transformerService;


    private List<BatchItem> _items;
    private Channel<JobProgress> _channel;

    public BatchTransformJobDescriptor(ITransformService transformService)
    {
      _transformerService = transformService;

      _items = new List<BatchItem>();
    }

    public IEnumerable<BatchItem> Items => _items;

    public bool SendsFeedback { get; set; }

    public override Task StartAsync(CancellationToken cancellationToken = default)
    {
      return StartAsync(_transformerService.ParseRecords(this, cancellationToken), cancellationToken);
    }

    public async Task AddItem(BatchItem item)
    {
      if (item == null) throw new ArgumentNullException("item");
      item.TotalRecords = await _transformerService.GetTotalRecords(item.FileName!, item.FirstLineHeader);
      _items.Add(item);
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
