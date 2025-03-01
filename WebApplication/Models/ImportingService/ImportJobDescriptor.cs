using Newtonsoft.Json;

namespace T2WebApplication.Models
{
  public enum ImportJobStatus
  {
    Created = 0,
    Running = 1,
    Paused = 2,
    Completed = 3,
    Cancelled = 4,
  }

  public class ImportJobDescriptor: IDisposable
  {
    private readonly Guid _id  = Guid.NewGuid();
    public Guid Id => _id;

    public ImportSourceModel Source { get; set; }

    public ImportJobStatus Status { get; set; }

    public int TotalRecords { get; set; }

    [JsonIgnore]
    public Task Task { get; set; }
    [JsonIgnore]
    public CancellationTokenSource CancellationTokenSource { get; set; }

    public string ConnectionId { get; set; }

    public void Dispose()
    {
      if (CancellationTokenSource != null)
      {
        CancellationTokenSource.Dispose();
        CancellationTokenSource = null;
      }
    }
  }
}
