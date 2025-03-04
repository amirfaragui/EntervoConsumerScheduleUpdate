using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Entrvo.Services
{

  public interface IFilesWatchOptions
  {
    string BaseDirectory { get; set; }
    IEnumerable<string> FileTypes { get; set; }
  }

  public class FilesWatchOptions: IFilesWatchOptions
  {
    public string BaseDirectory { get; set; }
    public IEnumerable<string> FileTypes { get; set; }
  }
}
