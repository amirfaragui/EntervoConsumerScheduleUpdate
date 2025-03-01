using Microsoft.AspNetCore.SignalR;

namespace T2WebApplication.Hubs
{
  public interface IConnectionEstablished
  {
    Task ConnectionEstablished(string connectionId);
  }

  public interface IViewService: IConnectionEstablished
  {
    Task ChildViewComitted(string connectionId);

    Task Success(string message);
    Task Error(string message);
  }

  public class ViewHub: Hub<IViewService>
  {
    public override async Task OnConnectedAsync()
    {
      await base.OnConnectedAsync();
      await Clients.Caller.ConnectionEstablished(Context.ConnectionId);
    }
  }
}
