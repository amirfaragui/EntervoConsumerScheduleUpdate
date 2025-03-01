using T2Importer.DAL.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace T2Importer.DAL
{
  [AuditIgnore]
  public class WebAccessAuditLog
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    public DateTimeOffset Time { get; set; }
    [StringLength(48)]
    public string ApplicationName { get; set; }
    [StringLength(24)]
    public string Method { get; set; }
    public string RequestUri { get; set; }
    [StringLength(64)]
    public string? Origin { get; set; }
    [StringLength(48)]
    public string? User { get; set; }
    public int StatusCode { get; set; }
    public int Elapsed { get; set; }
  }
}
