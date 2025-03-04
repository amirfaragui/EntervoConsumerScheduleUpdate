using MQTTnet.Protocol;
using MQTTnet.Server;

namespace Entrvo.Services
{
  public interface IRpcPayload
  {
    string Command { get; set; }
  }

  public interface IMqttService
  {
    Task PublishAsync(string topic, object payload, MqttQualityOfServiceLevel qos, bool retainMessage = false);
    //Task<T> PublishAsync<T>(Guid stationId, IRpcPayload payload, int timeout, MqttQualityOfServiceLevel qos);
    //Task NotifySignalRClients(ePark.Models.Event e);

    void AddMessageHandler(Func<InterceptingPublishEventArgs, Task> handler);
    void RemoveMessageHandler(Func<InterceptingPublishEventArgs, Task> handler);
    void AddSubscriptionHandler(Func<InterceptingSubscriptionEventArgs, Task> handler);
    void RemoveSubscriptionHandler(Func<InterceptingSubscriptionEventArgs, Task> handler);


  }
}