using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Entrvo.DAL.Attributes;

namespace Entrvo.DAL
{
  [Index(nameof(ClientRef), IsUnique = false)]
  public class Consumer
  {

    [Key]
    public Guid Id { get; set; }

    [StringLength(32)]
    public string? CardNumber { get; set; }

    [StringLength(32)]
    public string? ClientRef { get; set; }

    [StringLength(32)]
    public string? FirstName { get; set; }
    [StringLength(32)]
    public string? LastName { get; set; }
    [StringLength(24)] 
    public string? LPN1 { get; set; }
    [StringLength(24)] 
    public string? LPN2 { get; set; }
    [StringLength(24)] 
    public string? LPN3 { get; set; }


    [StringLength(64)]
    public string? Memo1 { get; set; }
    [StringLength(64)]
    public string? Memo2 { get; set; }
    [StringLength(64)]
    public string? Memo3 { get; set; }

    public DateTime? ValidUntil { get; set; }    // Valid To

    [Column(TypeName = "decimal(7,2)")]
    public decimal? Amount { get; set; }


    [StringLength(64)]
    public string? ContractId { get; set; }
    [StringLength(64)]
    public string? ConsumerId { get; set; }

    public bool IsActive { get; set; }

    [JsonIgnore, AuditIgnore]
    [Timestamp]
    public byte[] timestamp { get; set; }


    public Consumer()
    {
      Id = Guid.NewGuid();
      IsActive = true;
    }

    public void ParseNames()
    {
      if (string.IsNullOrEmpty(FirstName) && !string.IsNullOrEmpty(LastName))
      {
        var parts = LastName.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
          FirstName = parts[0];
          LastName = string.Join(" ", parts.Skip(1));
        }
      }
    }
  }
}
