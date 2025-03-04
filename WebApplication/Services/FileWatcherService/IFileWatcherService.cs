using Entrvo.Services.Models;

namespace Entrvo.Services
{
  public interface IFileWatcherService
  {
    void ChangeMonitoringFolder(string newFolder);
    event EventHandler<FileChangeEventArgs> OnFileReady;
  }
}
