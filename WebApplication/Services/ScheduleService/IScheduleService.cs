using T2WebApplication.Models;
using T2WebApplication.Services.Models;

namespace T2WebApplication.Services
{
  public interface IScheduleService
  {

    Task StartJob(Guid jobId, CancellationToken cancellation = default);

    bool TryGetJob(Guid jobId, out IJobDescriptor? jobDescriptor);
    bool IsFileInUse(string fileName);

    /// <summary>
    /// add parsing job for file uploaded from the web page UI frontend
    /// </summary>
    /// <param name="job"></param>
    /// <returns></returns>
    Task<IJobDescriptor> AddManualFileParsingJob(IFileSource job); 
    Task<IJobDescriptor> AddFilesParsingJob(params IFileSource[] files);
    
    Task<IJobDescriptor> AddPayrollFileDownloadJob();
    Task<IJobDescriptor> AddConsumersDownloadJob();
    Task<IJobDescriptor> AddConsumersPushJob();
    Task<IJobDescriptor> AddExport01ReportJOb();
    Task<IJobDescriptor> AddExport04ReportJOb();
  }
}
