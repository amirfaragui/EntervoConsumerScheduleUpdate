namespace T2WebApplication.Models
{
  public class ImportSourceModel
  {
    public string? FileName { get; set; }
    public Guid? UploadId { get; set; }

    public bool FirstLineHeader { get; set; }
    public string Delimiter { get; set; }

    //public int? DefaultSiteCode { get; set; }

    //public Guid? DefaultGroupId { get; set; }

    //public Guid? DefaultCompanyId { get; set; }

    //public Guid? DefaultTermId { get; set; }

    //public IdentifierType DefaultIdentifierType { get; set; }

    //public CardFunction CardFunction { get; set; }

    public ICollection<ColumnMappingModel> Mappings { get; set; }

    public Type? TargetType { get; set; }

    public ImportSourceModel()
    {
      Delimiter = ",";
      Mappings = new HashSet<ColumnMappingModel>();
    }
  }
}
