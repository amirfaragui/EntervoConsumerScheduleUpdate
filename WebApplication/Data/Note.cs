using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using T2Importer.DAL.Attributes;

namespace T2Importer.DAL
{
  public class Note: IDataEntity
  {
    [StringLength(20)]
    public string? Document { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsHistorical { get; set; }
    public DateTime? ModifyDate { get; set; }
    public string? NoteText { get; set; }
    [StringLength(20)]
    public string? NoteType { get; set; }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int NoteUID { get; set; }
    public int TableUIDofSourceObject { get; set; }

    [JsonIgnore, AuditIgnore]
    [Timestamp]
    public byte[] timestamp { get; set; }

    public Note()
    {
      TableUIDofSourceObject = 46;
    }

    public int GetId()
    {
      return NoteUID;
    }
  }

  [Table("CustomerNotes")]
  public class CustomerNote: Note
  {
    public int SourceObjectUID { get; set; }
    public CustomerNote()
    {
      TableUIDofSourceObject = 1;
    }
  }

  [Table("PermitNotes")]
  public class  PermitNote: Note
  {
    public int SourceObjectUID { get; set; }
    public PermitNote()
    {
      TableUIDofSourceObject = 10;
    }
  }

  //public class VehicleNote : Note
  //{
  //  public VehicleNote()
  //  {
  //    TableUIDofSourceObject = 46;
  //  }
  //}
}
