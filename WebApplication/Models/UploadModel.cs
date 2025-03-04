using System.ComponentModel;

namespace Entrvo.Models
{
  class ChunkMetaData
  {
    public string UploadUid { get; set; }
    public string FileName { get; set; }
    public string RelativePath { get; set; }
    public string ContentType { get; set; }
    public long ChunkIndex { get; set; }
    public long TotalChunks { get; set; }
    public long TotalFileSize { get; set; }
  }


  class FileResult
  {
    public bool uploaded { get; set; }
    public string fileUid { get; set; }
  }

  public class UploadModel: IFileSource
  {
    public string? FileName { get; set; }
    public Guid? UploadId { get; set; }

    [DisplayName("First Line Header")]
    public bool FirstLineHeader { get; set; }
    public string Delimiter { get; set; }

    public DateTime TimeStamp { get; set; }
    public long? TotalRecords { get; set; }

    public UploadModel()
    {
      Delimiter = "\t";
      TimeStamp = DateTime.Now;
    }
  }

  public class UploadResponse
  {
    public bool Success { get; set; }
    public Guid JobId { get; set; }
    public string Message { get; set; }
  }

}
