using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using Entrvo.DAL.Attributes;

namespace Entrvo.DAL
{
  public class Setting
  {
    [Key]
    [StringLength(64)]
    public string Key { get; set; }

    [StringLength(256)]
    public string Value { get; set; }

    [JsonIgnore, AuditIgnore]
    [Timestamp]
    public byte[]? RowVersion { get; set; }
  }
}
