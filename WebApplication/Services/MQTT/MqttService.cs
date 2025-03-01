using Microsoft.Extensions.Primitives;
using MQTTnet;
using MQTTnet.AspNetCore;
using MQTTnet.Protocol;
using MQTTnet.Server;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace T2WebApplication.Services
{
  public class MqttService : IMqttService
  {
    private MqttServer _mqtt;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MqttService> _logger;
    private readonly JsonSerializerSettings _jsonSettings;
    public MqttService(IServiceScopeFactory serviceScopeFactory,
                       IConfiguration config,
                       ILogger<MqttService> logger)

    {
      _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));

      _jsonSettings = new JsonSerializerSettings()
      {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
      };
      _jsonSettings.Converters.Add(new StringEnumConverter());


      ChangeToken.OnChange(() => config.GetReloadToken(), async (state) => await InvokeChange(state), config);
    }

    public void ConfigureMqttServerOptions(AspNetMqttServerOptionsBuilder options)
    {
      options.WithDefaultEndpoint();
      options.WithDefaultEndpointPort(1883);
      options.WithDefaultCommunicationTimeout(TimeSpan.FromSeconds(5));
    }

    public void ConfigureMqttServer(MqttServer mqtt)
    {
      _mqtt = mqtt;
      _mqtt.ValidatingConnectionAsync += ValidateConnectionAsync;
      _mqtt.InterceptingPublishAsync += InterceptApplicationMessagePublishAsync;
      _mqtt.InterceptingSubscriptionAsync += InterceptSubscriptionAsync;

      _mqtt.ClientConnectedAsync += HandleClientConnectedAsync;
      _mqtt.ClientDisconnectedAsync += HandleClientDisconnectedAsync;
    }

    public void AddMessageHandler(Func<InterceptingPublishEventArgs, Task> handler)
    {
      _mqtt.InterceptingPublishAsync += handler;
    }
    public void RemoveMessageHandler(Func<InterceptingPublishEventArgs, Task> handler)
    {
      _mqtt.InterceptingPublishAsync -= handler;
    }

    public void AddSubscriptionHandler(Func<InterceptingSubscriptionEventArgs, Task> handler)
    {
      _mqtt.InterceptingSubscriptionAsync += handler;
    }
    public void RemoveSubscriptionHandler(Func<InterceptingSubscriptionEventArgs, Task> handler)
    {
      _mqtt.InterceptingSubscriptionAsync -= handler;
    }

    private async Task HandleClientConnectedAsync(ClientConnectedEventArgs args)
    {
      //if (TryGetUniqueIdFromSubject(args.ClientId, out var stationId))
      //{
      //  if (_stations.TryAdd(stationId))
      //  {
      //    using var scope = _serviceScopeFactory.CreateScope();
      //    using var dbContext = scope.ServiceProvider.GetService<EparkContext>();

      //    var station = await dbContext.Stations.FindAsync(stationId);
      //    if (station != null)
      //    {
      //      station.LastSeen = DateTimeOffset.Now;
      //      await dbContext.SaveChangesAsync();


      //      var eventService = scope.ServiceProvider.GetService<IEventService>();
      //      var e = new ePark.Models.Event()
      //      {
      //        Type = EventType.ConnectionEstablished,
      //        Station = station.Name,
      //      };
      //      await eventService.RegisterServerEvent(e);

      //      var message = new MqttApplicationMessageBuilder()
      //        .WithTopic($"connected/{stationId:N}")
      //        .WithPayload(string.Empty)
      //        .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
      //        .WithRetainFlag(true)
      //        .Build();

      //      await _mqtt.InjectApplicationMessage(new InjectedMqttApplicationMessage(message) { SenderClientId = "ePark Server" });
      //    }
      //  }
      //}
    }

    private async Task HandleClientDisconnectedAsync(ClientDisconnectedEventArgs args)
    {
      //if (TryGetUniqueIdFromSubject(args.ClientId, out var stationId))
      //{
      //  _stations.TryRemove(stationId);

      //  using var scope = _serviceScopeFactory.CreateScope();
      //  using var dbContext = scope.ServiceProvider.GetService<EparkContext>();

      //  var station = await dbContext.Stations.FindAsync(stationId);
      //  if (station != null)
      //  {
      //    var eventService = scope.ServiceProvider.GetService<IEventService>();
      //    var e = new ePark.Models.Event()
      //    {
      //      Type = EventType.ConnectionDown,
      //      Station = station.Name,
      //    };
      //    await eventService.RegisterServerEvent(e);

      //    var message = new MqttApplicationMessageBuilder()
      //      .WithTopic($"disconnected/{stationId:N}")
      //      .WithPayload(string.Empty)
      //      .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtMostOnce)
      //      .WithRetainFlag(true)
      //      .Build();

      //    await _mqtt.InjectApplicationMessage(new InjectedMqttApplicationMessage(message) { SenderClientId = "ePark Server" });
      //  }
      //}
    }

    async Task ValidateConnectionAsync(ValidatingConnectionEventArgs e /* MqttConnectionValidatorContext context*/)
    {
      //if (e.ClientId.StartsWith("MQTTnet.RPC:") || e.ClientId.StartsWith("hub:") || e.ClientId.StartsWith($"{Environment.MachineName}-TouchScreen"))
      //{
      //  e.ReasonCode = MqttConnectReasonCode.Success;
      //  return;
      //}

      //if (TryGetUniqueIdFromSubject(e.ClientId, out var stationId))
      //{
      //  if (_stations.Contains(stationId))
      //  {
      //    e.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
      //    e.ReasonString = "There's a station with the same id already running.";
      //    return;
      //  }

      //  using var scope = _serviceScopeFactory.CreateScope();
      //  using var dbContext = scope.ServiceProvider.GetService<EparkContext>();

      //  var station = await dbContext.Stations.FindAsync(stationId);
      //  if (station != null)
      //  {
      //    e.ReasonCode = MqttConnectReasonCode.Success;
      //    return;
      //  }
      //}

      //e.ReasonString = "There's a no station configured with the id specified.";
      //e.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;

      e.ReasonCode = MqttConnectReasonCode.Success;

    }

#pragma warning disable 4014
    private async Task InterceptSubscriptionAsync(InterceptingSubscriptionEventArgs e)
    {
      //if (e.TopicFilter.Topic.StartsWith("status/"))
      //{
      //  var retainedMessages = await _mqtt.GetRetainedMessagesAsync();
      //  foreach (var retainedMessage in retainedMessages)
      //  {
      //    if (MqttTopicFilterComparer.Compare(retainedMessage.Topic, e.TopicFilter.Topic) == MqttTopicFilterCompareResult.IsMatch)
      //    {
      //      return;
      //    }
      //  }

      //  if (TryGetUniqueIdFromSubject(e.TopicFilter.Topic, out var stationId))
      //  {
      //    Task.Run(async () =>
      //    {
      //      var requestTopic = $"MQTTnet.RPC/{stationId:N}/status/{Guid.NewGuid():N}";
      //      var request = new Areas.Api.Models.RemoteRequest() { Command = "status-update" };
      //      await PublishAsync(requestTopic, request, MqttQualityOfServiceLevel.AtLeastOnce, false);
      //    });
      //  }
      //}
    }
#pragma warning restore 4014

    private async Task InterceptApplicationMessagePublishAsync(InterceptingPublishEventArgs e)
    {
      //try
      //{
      //  var topicParts = e.ApplicationMessage.Topic.Split('/');
      //  switch (topicParts[0].ToLower())
      //  {
      //    case "heartbeat":
      //      OnHeartBeatMessageReceived(e.ApplicationMessage);
      //      break;

      //    case "event":
      //      OnStationEventReceived(e.ApplicationMessage);
      //      break;

      //    case "checkin":
      //      OnCustomerEntered(e.ApplicationMessage);
      //      break;

      //    case "paid":
      //      OnCustomerPaid(e.ApplicationMessage);
      //      break;

      //    case "checkout":
      //      OnCustomerDeparted(e.ApplicationMessage);
      //      break;

      //    case "lostticket":
      //      OnLostTicket(e.ApplicationMessage);
      //      break;

      //    case "daypass":
      //      OnDayPass(e.ApplicationMessage);
      //      break;

      //    case "merchandise":
      //      OnMerchandisePurchased(e.ApplicationMessage);
      //      break;

      //    case "renew":
      //      OnRenewSubscriber(e.ApplicationMessage);
      //      break;

      //    case "paydiaplay":
      //      OnPayAndDisplayAsync(e.ApplicationMessage);
      //      break;

      //    case "counter":
      //      OnCountActivityMessageReceived(e.ApplicationMessage);
      //      break;

      //    case "void":
      //      OnPostClearTransaction(e.ApplicationMessage);
      //      break;

      //    case "logoff":
      //      OnPosCashierLogoff(e.ApplicationMessage);
      //      break;

      //    case "usedcoupons":
      //      OnCouponsUsed(e.ApplicationMessage);
      //      break;

      //    case "loopcount":
      //      return OnLoopCountMessageReceived(e.ApplicationMessage);
      //  }
      //}
      //catch (Exception ex)
      //{
      //  _logger.LogError(ex.ToString());
      //}

      //return Task.CompletedTask;

    }

    #region IMqttService
    public async Task PublishAsync(string topic, object payload, MqttQualityOfServiceLevel qos, bool retainMessage)
    {
      var message = new MqttApplicationMessageBuilder()
        .WithTopic(topic)
        .WithPayload(JsonConvert.SerializeObject(payload, _jsonSettings))
        .WithQualityOfServiceLevel(qos)
        .WithRetainFlag(retainMessage)
        .Build();

      await _mqtt.InjectApplicationMessage(new InjectedMqttApplicationMessage(message) { SenderClientId = "ePark Server" });
    }

    //public async Task<TResult> PublishAsync<TResult>(Guid stationId, IRpcPayload payload, int timeout, MqttQualityOfServiceLevel qos)
    //{
    //  TResult result = default;

    //  using (var client = new MqttFactory().CreateMqttClient())
    //  {
    //    var options = new MqttClientOptionsBuilder()
    //      .WithClientId(string.Format("MQTTnet.RPC:{0}", Guid.NewGuid()))
    //      .WithTimeout(TimeSpan.FromSeconds(5))
    //      .WithTcpServer("localhost")
    //      .WithKeepAlivePeriod(TimeSpan.FromMinutes(1))
    //      .Build();

    //    await client.ConnectAsync(options);

    //    var rpcOptions = new MqttRpcClientOptionsBuilder()
    //      .WithTopicGenerationStrategy(new StationRpcTopicGenerator(stationId))
    //      .Build();

    //    using (var rpcClinet = new MqttRpcClient(client, rpcOptions))
    //    {
    //      var jsonPayload = JsonConvert.SerializeObject(payload, _jsonSettings);
    //      var bytesPayload = Encoding.UTF8.GetBytes(jsonPayload);

    //      var response = await rpcClinet.ExecuteAsync(TimeSpan.FromMilliseconds(timeout), payload.Command, bytesPayload, qos);
    //      var jsonResponse = Encoding.UTF8.GetString(response);

    //      result = JsonConvert.DeserializeObject<TResult>(jsonResponse, _jsonSettings);
    //    }

    //    await client.DisconnectAsync();
    //  }

    //  return result;
    //}


    #endregion


    private Task OnHeartBeatMessageReceived(MqttApplicationMessage message)
    {
      return Task.CompletedTask;
    }


    private async Task InvokeChange(IConfiguration config)
    {
      _logger.LogInformation("Configuration changed");
      await PublishAsync("settings", new { }, MqttQualityOfServiceLevel.ExactlyOnce, false);
    }

    #region Helper
    private bool TryGetUniqueIdFromSubject(string clientId, out Guid uuid)
    {
      uuid = default;
      var topicParts = clientId.Split(':', '/');
      if (topicParts.Length == 2)
      {
        if (Guid.TryParse(topicParts[1], out uuid))
        {
          return true;
        }
      }
      return false;
    }

    private bool TryParseMqttMessage<T>(MqttApplicationMessage e, out T result)
    {
      result = default;
      var payloadString = Encoding.UTF8.GetString(e.Payload);
      try
      {
        result = JsonConvert.DeserializeObject<T>(payloadString, _jsonSettings);
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        _logger.LogDebug(payloadString);
        return false;
      }
    }
    #endregion
  }
}

