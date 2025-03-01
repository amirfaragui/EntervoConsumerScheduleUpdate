using SnB.Models;

namespace T2WebApplication.Services
{
  public interface IApiClient
  {
    Task Initialize(string? url = null, Credential? credential = null);
    IObservable<ConsumerDetail> GetConsumerDetails(int? contractId, CancellationToken cancellationToken = default);
    IAsyncEnumerable<ConsumerDetail> GetConsumerDetailsAsync(int? contractId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Cashier>> GetCashiersAsync(CancellationToken cancellationToken = default);
    Task<Shift> GetActiveShiftAsync(string cashierContractId, string cashierConsumerId, CancellationToken cancellationToken = default);
    Task<Shift> CreateShiftAsync(Cashier cashier, Device device, CancellationToken cancellationToken = default);
    Task<IEnumerable<Device>> GetDevicesAsync(CancellationToken cancellationToken = default);
    Task<Transaction> PostPayment(TransactionDetail transaction, CancellationToken cancellationToken = default);
    Task<ConsumerDetail?> GetConsumerAsync(string contractId, string consumerId, CancellationToken cancellationToken = default);
    Task<bool> UpdateConsumerAsync(ConsumerDetail consumer, CancellationToken cancellationToken = default);
  }
}
