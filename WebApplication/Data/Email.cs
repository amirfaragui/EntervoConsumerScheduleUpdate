using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entrvo.DAL.Attributes;

namespace Entrvo.DAL
{
  public class Email: IDataEntity<int>
  {
    public int CustomerUID { get; set; }
    [Range(1, 100)]
    public int EffectiveRank { get; set; }
    [StringLength(64), DataType(DataType.EmailAddress)]
    public string? EmailAddress { get; set; }
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int EmailAddressUID { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsHistorical { get; set; }
    public DateTime? ModifyDate { get; set; }
    [Range(1, 100)] 
    public int Priority { get; set; }
    public int SourceType { get; set; }
    public DateTime? StartDate { get; set; }
    [StringLength(20)]
    public string? Type { get; set; }

    [JsonIgnore, AuditIgnore]
    [Timestamp]
    public byte[] timestamp { get; set; }

    public Email ()
    {
      ModifyDate = DateTime.Now;
      EffectiveRank = 1;
      Priority = 1;
    }

    public int GetId()
    {
      return EmailAddressUID;
    }

  }
}
