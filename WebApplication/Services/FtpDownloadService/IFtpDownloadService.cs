namespace Entrvo.Services
{
  public interface IFtpDownloadService
  {
    Task<bool> StartDownloadAsync(CancellationToken cancellationToken);
  }
}
