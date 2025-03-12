using Entrvo.Api.Models;
using EntrvoWebApp.Services.Models;

namespace EntrvoWebApp.Services
{
  public interface IEntrvoConsumerService
  {
    Task<ConsumerDetails?> UpdateConsumerAsync(EntrvoRecord record, CancellationToken cancellationToken);
  }
}
