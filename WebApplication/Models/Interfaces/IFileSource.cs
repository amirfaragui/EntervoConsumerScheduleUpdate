using System.ComponentModel;

namespace T2WebApplication.Models
{
  public interface IFileSource
  {
    public string? FileName { get; set; }
    public Guid? UploadId { get; set; }

    public bool FirstLineHeader { get; set; }
    public string Delimiter { get; set; }

    public DateTime TimeStamp { get; set; }
    public long? TotalRecords { get; set; }
  }
}
