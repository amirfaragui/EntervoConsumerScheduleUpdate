using System.Threading.Channels;
using Entrvo.Models;

namespace Entrvo.Services.Models
{
  public enum JobStatus
  {
    Created = 0,
    Running = 1,
    Paused = 2,
    Completed = 3,
    Cancelled = 4,
    Fault = 5,
  }

  public interface IJobDescriptor
  {
    Guid JobId { get; }
    JobStatus Status { get; set; }
    Task Completion { get; }
    Task StartAsync(CancellationToken cancellationToken);
    DateTime Timestamp { get; }
  }

  public interface IJobFeedback
  {
    bool SendsFeedback { get; set; }
    Channel<JobProgress> CreateChannel();
  }
}
