namespace T2WebApplication.Services
{
  public interface IFtpDownloadService
  {
    Task<bool> StartDownloadAsync(CancellationToken cancellationToken);
  }
}
