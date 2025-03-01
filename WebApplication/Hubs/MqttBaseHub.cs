using Microsoft.AspNetCore.SignalR;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;
using System.Threading.Channels;

namespace T2WebApplication.Hubs
{
  public abstract class MqttBaseHub<T>: Hub<IConnectionEstablished> where T: class
  {
    protected readonly ILogger _logger;
    protected IManagedMqttClient _mqttClient;

    private CancellationToken _cancellationToken;
    private IList<MqttTopicFilter> _topics;
    private Channel<T> _channel;

    public MqttBaseHub(ILogger logger)
    {
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
      SerializerSettings = new JsonSerializerSettings
      {
        ContractResolver = new DefaultContractResolver()
      };
    }

    public JsonSerializerSettings SerializerSettings { get; set; }

    public override async Task OnConnectedAsync()
    {
      await base.OnConnectedAsync();
      await Clients.Caller.ConnectionEstablished(Context.ConnectionId);
    }

    protected override async void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      await UnregisterSubscription();
    }

    protected async Task RegisterSubscription()
    {
      _mqttClient = new MqttFactory().CreateManagedMqttClient();
      _mqttClient.ApplicationMessageReceivedAsync += OnApplicationMessageReceived;
      _mqttClient.ConnectedAsync += OnMqttConnected;

      var options = new ManagedMqttClientOptionsBuilder()
        .WithAutoReconnectDelay(TimeSpan.FromSeconds(15))
        .WithClientOptions(new MqttClientOptionsBuilder()
          .WithClientId($"hub:{Context.ConnectionId}")
          .WithKeepAlivePeriod(TimeSpan.FromSeconds(30))
          .WithCleanSession()
          .WithTcpServer("localhost")
          .Build())
        .Build();

      await _mqttClient.StartAsync(options);
    }

    protected virtual async Task UnregisterSubscription()
    {
      if (_mqttClient != null)
      {
        try
        {
          if (_topics != null && _topics.Any())
          {
            await _mqttClient?.UnsubscribeAsync(_topics.Select(i => i.Topic).ToArray());
          }
          await _mqttClient?.StopAsync();
          _mqttClient?.Dispose();
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
        finally
        {
          _mqttClient = null;
        }
      }
    }

    protected virtual async Task OnMqttConnected(EventArgs e)
    {
      _topics = SetupSubscriptions();
      await _mqttClient.SubscribeAsync(_topics);
    }

    protected virtual IList<MqttTopicFilter> SetupSubscriptions()
    {
      return new List<MqttTopicFilter>();
    }

    protected virtual Task OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
      return Task.CompletedTask;
    }

    protected bool TryParseMqttMessage<TResult>(MqttApplicationMessage e, out TResult result)
    {
      result = default;
      var payloadString = Encoding.UTF8.GetString(e.Payload);
      try
      {
        result = JsonConvert.DeserializeObject<TResult>(payloadString, SerializerSettings);
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        return false;
      }
    }

    protected virtual ChannelReader<T> Stream(CancellationToken cancellationToken)
    {
      _cancellationToken = cancellationToken;

      _cancellationToken.Register(async () =>
      {
        await UnregisterSubscription();
      });

      Task.Run(() => RegisterSubscription());

      _channel = Channel.CreateUnbounded<T>();
      return _channel;
    }

    protected virtual async Task PublishAsync(T item)
    {
      if (!_cancellationToken.IsCancellationRequested)
      {
        try
        {
          await _channel.Writer.WriteAsync(item, _cancellationToken);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex.ToString());
        }
      }
    }

  }
}
