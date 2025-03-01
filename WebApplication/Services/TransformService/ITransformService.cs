using T2WebApplication.Services.Models;

namespace T2WebApplication.Services
{
  public interface ITransformService: IDisposable
  {
    Task ParseRecords(BatchTransformJobDescriptor jobDescriptor, CancellationToken cancellationToken = default);

    Task<int> GetTotalRecords(string fileName, bool firstLineHeader);
  }
}
