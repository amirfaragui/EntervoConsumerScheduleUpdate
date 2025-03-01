using T2Importer.DAL.Attributes;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T2Importer.DAL
{
  public enum AuditType
  {
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 3
  }

  [AuditIgnore]
  public class Audit
  {


    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string Type { get; set; }
    [StringLength(128)]
    public string? TableName { get; set; }
    public DateTimeOffset DateTime { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
  }

  public class AuditEntry
  {
    public AuditEntry(EntityEntry entry)
    {
      Entry = entry;
    }
    public EntityEntry Entry { get; }
    public string? UserId { get; set; }
    public string? TableName { get; set; }
    public Dictionary<string, object> KeyValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> OldValues { get; } = new Dictionary<string, object>();
    public Dictionary<string, object> NewValues { get; } = new Dictionary<string, object>();
    public AuditType AuditType { get; set; }
    public List<string> ChangedColumns { get; } = new List<string>();
    public Audit ToAudit()
    {
      var audit = new Audit();
      audit.UserId = UserId;
      audit.Type = AuditType.ToString();
      audit.TableName = TableName;
      audit.DateTime = DateTime.Now;
      audit.PrimaryKey = JsonConvert.SerializeObject(KeyValues);
      audit.OldValues = OldValues.Count == 0 ? null : JsonConvert.SerializeObject(OldValues);
      audit.NewValues = NewValues.Count == 0 ? null : JsonConvert.SerializeObject(NewValues);
      audit.AffectedColumns = ChangedColumns.Count == 0 ? null : JsonConvert.SerializeObject(ChangedColumns);
      return audit;
    }
  }
}
