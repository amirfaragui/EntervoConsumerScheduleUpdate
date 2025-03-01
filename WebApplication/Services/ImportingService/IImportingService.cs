using T2Importer.DAL;
using T2WebApplication.Models;

namespace T2WebApplication.Services
{
  public interface IImportingService
  {
    Task<ImportJobDescriptor> AddJob(ImportSourceModel job);
    Task StartJob(Guid jobId);

    bool TryGetJob(Guid jobId, out ImportJobDescriptor? jobDescriptor);

    bool TryGetJob<T>(out ImportJobDescriptor? jobDescriptor) where T : class, IDataEntity;

    bool IsFileInUse(string fileName);
  }
}
