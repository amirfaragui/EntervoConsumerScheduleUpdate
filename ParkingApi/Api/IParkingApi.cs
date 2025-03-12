using Entrvo.Api;
using Entrvo.Api.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Entrvo.Services
{

  public enum ConsumerFilter
  {
    MinContractId,
    MaxContractId,
    MinId,
    MaxId,
    ValidFromConsumer,
    ValidUntilConsumer,
    ValidFromContract,
    ValidUntilContract,
    Status,
    Delete,
    Lpn1,
    Lpn2,
    Lpn3,
    Cardno,
    Name,
    IgnorePresentce,
    InstanceId,
  }

  public interface IParkingApi
  {
    void Initialize(ApiOptions options);
    Task<Cashier[]> GetCashiersAsync(CancellationToken cancellationToken = default);
    Task<Shift> GetActiveShiftAsync(string cashierContractId, string cashierConsumerId, CancellationToken cancellationToken = default);
    Task<Shift> CreateShiftAsync(Cashier cashier, Device device, CancellationToken cancellationToken = default);
    Task<Device[]> GetDevicesAsync(CancellationToken cancellationToken = default);
    Task<Transaction> PostPayment(TransactionDetail transaction, CancellationToken cancellationToken = default);
    Task<UsageProfile[]> GetUsageProfilesAsync(CancellationToken cancellationToken = default);



    //IObservable<ConsumerDetail> GetConsumerDetails(int? contractId, CancellationToken cancellationToken = default);
    //IAsyncEnumerable<ConsumerDetails> GetAllConsumerDetailsAsync(int? contractId, [EnumeratorCancellation] CancellationToken cancellationToken = default);
    Task<ConsumerDetails?> GetConsumerDetailsAsync(string contractId, string consumerId, CancellationToken cancellationToken = default);

    IAsyncEnumerable<Consumer> FindConsumersAsync(IDictionary<ConsumerFilter, object> filters, CancellationToken cancellationToken = default);

    IAsyncEnumerable<ConsumerDetails> FindConsumerDetailssAsync(IDictionary<ConsumerFilter, object> filters, CancellationToken cancellationToken = default);

    Task<int> CreateConsumerAsync(int templateId, ConsumerDetails consumer, CancellationToken cancellationToken = default);

    Task<bool> UpdateConsumerAsync(ConsumerDetails consumer, CancellationToken cancellationToken = default);
  }
}
