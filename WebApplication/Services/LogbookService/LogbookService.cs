using Serilog.Core;
using Serilog.Events;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace Entrvo.Services
{
  public class LogbookService : ILogEventSink, ILogbookService
  {
    private readonly IFormatProvider _formatProvider;
    private readonly ConcurrentDictionary<string, Channel<string>> _channels;

    public LogbookService(IFormatProvider formatProvider)
    {
      _channels = new ConcurrentDictionary<string, Channel<string>>();
      _formatProvider = formatProvider;
    }


    public void Emit(LogEvent logEvent)
    {
      if (logEvent.Properties.ContainsKey("SourceContext"))
      {
        var context = logEvent.Properties["SourceContext"];
        var value = context.ToString().Trim('"');
        if (value.StartsWith("Entrvo"))
        {
          var message = logEvent.RenderMessage(_formatProvider);
          foreach (var channel in _channels.Values)
          {
            channel.Writer.TryWrite(message);
          }
        }
      }
    }

    public Channel<string> CreateChannel(string connectionId)
    {
      if (_channels.TryGetValue(connectionId, out var channel)) return channel;
      channel = Channel.CreateUnbounded<string>();
      _channels.TryAdd(connectionId, channel);
      return channel;
    }

    public void ReleaseChannel(string connectionId)
    {
      if (_channels.TryRemove(connectionId, out var channel))
      {
        channel.Writer.Complete();
      }
    }
  }


}
