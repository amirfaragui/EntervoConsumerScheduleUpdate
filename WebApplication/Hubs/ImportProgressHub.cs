using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Packets;
using System.Threading.Channels;
using T2WebApplication.Models;

namespace T2WebApplication.Hubs
{
  public class ImportProgressHub : MqttBaseHub<ImportJobProgress>
  {
    private Guid _jobId;

    public ImportProgressHub(ILogger<ImportProgressHub> logger) : base(logger)
    {
    }

    public ChannelReader<ImportJobProgress> Progress(Guid jobId, CancellationToken cancellationToken)
    {
      _jobId = jobId;
      return Stream(cancellationToken);
    }

    protected override IList<MqttTopicFilter> SetupSubscriptions()
    {
      var topics = new List<MqttTopicFilter>();
      topics.Add(new MqttTopicFilterBuilder()
        .WithTopic($"progress/{_jobId:N}")
        .Build());
      return topics;
    }


    protected override async Task OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
    {
      await OnEventMessageReceived(e.ApplicationMessage);
    }

    private async Task OnEventMessageReceived(MqttApplicationMessage message)
    {
      try
      {
        var payload = message.ConvertPayloadToString();
        if (TryParseMqttMessage(message, out ImportJobProgress e))
        {
          await PublishAsync(e);
        }
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
      }
    }
  }

}
