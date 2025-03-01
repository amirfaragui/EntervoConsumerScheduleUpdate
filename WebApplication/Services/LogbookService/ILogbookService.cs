using System.Threading.Channels;

namespace T2WebApplication.Services
{
  public interface ILogbookService
  {    
    Channel<string> CreateChannel(string connectionId);
    void ReleaseChannel(string connectionId);
  }
}
