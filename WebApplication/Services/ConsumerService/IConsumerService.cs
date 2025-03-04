namespace Entrvo.Services
{
  public interface IConsumerService
  {
    Task<bool> DownloadConsumersAsync(CancellationToken cancellation = default);

    Task<bool> UpdateConsumersAsync(CancellationToken cancellation = default);

    Task<bool> GenerateO1ExportFile(CancellationToken cancellation = default);

    Task<bool> GenerateO4ExportFile(CancellationToken cancellation = default);
  }
}
