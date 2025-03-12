namespace Entrvo.Models
{
  public class ImportSourceModel
  {
    public string? FileName { get; set; }
    public Guid? UploadId { get; set; }

    public bool FirstLineHeader { get; set; }
    public string Delimiter { get; set; }

    public ICollection<ColumnMappingModel> Mappings { get; set; }

    public Type? TargetType { get; set; }

    public Func<string[], object>? KeySelector { get; set; }  

    public ImportSourceModel()
    {
      Delimiter = ",";
      Mappings = new HashSet<ColumnMappingModel>();
    }
  }
}
