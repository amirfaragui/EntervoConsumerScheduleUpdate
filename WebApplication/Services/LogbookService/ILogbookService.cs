using System.Threading.Channels;

namespace Entrvo.Services
{
  public interface ILogbookService
  {    
    Channel<string> CreateChannel(string connectionId);
    void ReleaseChannel(string connectionId);
  }
}
