using Entrvo.Api.Models;
using EntrvoWebApp.Services.Models;

namespace EntrvoWebApp.Services
{
  public interface IEntrvoConsumerService
  {
    Task<ConsumerDetails?> UpdateConsumerAsync(EntrvoRecord record,  CancellationToken cancellationToken);
    Task<ConsumerDetails?> UpdateConsumerAsync(string contractId, string consumerId, EntrvoRecord record, CancellationToken cancellationToken);
  }
}
