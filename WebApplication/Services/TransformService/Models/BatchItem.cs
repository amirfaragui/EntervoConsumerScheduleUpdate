using System.Threading.Channels;
using Entrvo.Models;

namespace Entrvo.Services.Models
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
