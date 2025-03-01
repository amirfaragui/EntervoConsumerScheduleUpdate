using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;
using T2WebApplication.Services;

namespace T2WebApplication.Hubs
{
  public class LogbookHub : Hub
  {
    private readonly ILogger<LogbookHub> _logger;
    private readonly ILogbookService _logbookService;

    public LogbookHub(ILogger<LogbookHub> logger, ILogbookService logbookService)
    {
      _logger = logger;
      _logbookService = logbookService;
    }

    public override async Task OnConnectedAsync()
    {
      await base.OnConnectedAsync();
      await Clients.Caller.SendAsync("ConnectionEstablished", $"{Context.ConnectionId}");
    }

    public ChannelReader<string> Stream(string connectionId, CancellationToken cancellationToken)
    {
      cancellationToken.Register(() =>
      {
        _logbookService.ReleaseChannel(connectionId);
      });
      return _logbookService.CreateChannel(connectionId);
    }
  }
}
