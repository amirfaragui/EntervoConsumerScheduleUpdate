using System.Threading.Channels;
using T2WebApplication.Models;

namespace T2WebApplication.Services.Models
{
  public class BatchItem : IFileSource
  {
    public Guid? UploadId { get; set; }
    public DateTime TimeStamp { get; set; }
    public string? FileName { get; set; }
    public long? Size { get; set; }
    public long? TotalRecords { get; set; }
    public bool FirstLineHeader { get; set; }
    public string Delimiter { get; set; }
  }
}
