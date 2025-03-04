using Entrvo.Services.Models;

namespace Entrvo.Services
{
  public interface ITransformService: IDisposable
  {
    Task ParseRecords(BatchTransformJobDescriptor jobDescriptor, CancellationToken cancellationToken = default);

    Task<int> GetTotalRecords(string fileName, bool firstLineHeader);
  }
}
