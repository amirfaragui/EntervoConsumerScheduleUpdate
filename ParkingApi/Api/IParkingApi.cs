using Entrvo.Api;
using Entrvo.Api.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Entrvo.Services
{
  public interface IParkingApi
  {
    void Initialize(ApiOptions options);
    Task<Cashier[]> GetCashiersAsync(CancellationToken cancellationToken = default);
    Task<Shift> GetActiveShiftAsync(string cashierContractId, string cashierConsumerId, CancellationToken cancellationToken = default);
    Task<Shift> CreateShiftAsync(Cashier cashier, Device device, CancellationToken cancellationToken = default);
    Task<Device[]> GetDevicesAsync(CancellationToken cancellationToken = default);
    Task<Transaction> PostPayment(TransactionDetail transaction, CancellationToken cancellationToken = default);



    //IObservable<ConsumerDetail> GetConsumerDetails(int? contractId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ConsumerDetails> GetAllConsumerDetailsAsync(int? contractId, [EnumeratorCancellation] CancellationToken cancellationToken = default);
    Task<ConsumerDetails?> GetConsumerDetailsAsync(string contractId, string consumerId, CancellationToken cancellationToken = default);



    Task<bool> UpdateConsumerAsync(ConsumerDetails consumer, CancellationToken cancellationToken = default);
  }
}
