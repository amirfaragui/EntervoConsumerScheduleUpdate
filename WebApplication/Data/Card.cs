using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Entrvo.DAL.Attributes;

namespace Entrvo.DAL
{
  public enum CardStatus
  {
    NoChange = 0,
    Created = 1,
    Modified = 2,
    Removed = 3,
    Synchronized = 5,
    Error = 10,
    Duplicated = 11,
  }

  public class Card
  {
    [Key]
    [StringLength(32)]
    public string ClientRef { get; set; }

    [Column(TypeName = "decimal(7,2)")]
    public decimal Amount { get; set; }

    [Column(TypeName = "smalldatetime")]

    public DateTime? ValidUntil { get; set;}    // Valid To

    [StringLength(64)]
    public string? ContractId { get; set; }
    [StringLength(64)]
    public string? ConsumerId { get; set; }

    [AuditIgnore]
    public CardStatus Status { get; set; }

    [AuditIgnore]
    public DateTime TimeModified { get; set; }


    [AuditIgnore]
    public int Version { get; set; } // Batch Number

    [JsonIgnore, AuditIgnore]
    [Timestamp]
    public byte[] timestamp { get; set; }

  }
}
