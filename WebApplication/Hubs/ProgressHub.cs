using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;
using T2WebApplication.Models;
using T2WebApplication.Services;
using T2WebApplication.Services.Models;

namespace T2WebApplication.Hubs
{
  public class ProgressHub : Hub
  {
    private readonly ILogger<ProgressHub> _logger;
    private readonly IScheduleService _transformService;

    public ProgressHub(ILogger<ProgressHub> logger, IScheduleService transformService)
    {
      _logger = logger;
      _transformService = transformService;
    }

    public override async Task OnConnectedAsync()
    {
      await base.OnConnectedAsync();
      await Clients.Caller.SendAsync("ConnectionEstablished", $"{Context.ConnectionId}");
    }

    public ChannelReader<JobProgress>? Progress(Guid jobId, CancellationToken cancellationToken)
    {
      if (_transformService.TryGetJob(jobId, out var job))
      {
        if (job is IJobFeedback feedback)
        {
          return feedback.CreateChannel();
        }
      }
      return null;
    }
  }
}
