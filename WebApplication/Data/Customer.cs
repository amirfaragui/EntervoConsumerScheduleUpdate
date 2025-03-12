using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entrvo.DAL.Attributes;

namespace Entrvo.DAL
{
  public class Customer : IDataEntity<int>
  {
    [Column(TypeName = "decimal(7,2)"), Display(Name = "Account Balance")]
    public decimal AccountBalance { get; set; }


    [StringLength(48), Display(Name = "Allotment Group")]
    public string? AllotmentGroup { get; set; }


    [Column(TypeName = "decimal(7,2)"), Display(Name = "Balance Due")]
    public decimal BalanceDue { get; set; }


    [StringLength(48)]
    public string? Classification { get; set; }


    [StringLength(48), Display(Name = "Allotment Group")]
    public string? CustomerType { get; set; }


    [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Display(Name = "Customer ID")]
    public int CustomerUID { get; set; }
    public bool DisallowChecks { get; set; }


    [StringLength(48), Display(Name = "Employee ID")]
    public string? EmployeeID { get; set; }


    [StringLength(48), Display(Name = "First Name")]
    public string? FirstName { get; set; }


    [NotMapped]
    public string FullName => $"{FirstName}, {LastName} {MiddleName}";

    [StringLength(64), Display(Name = "Group Name")]
    public string? GroupName { get; set; }


    public bool HasLetter { get; set; }
    public bool HasNote { get; set; }
    public bool HasPendingLetter { get; set; }

    [StringLength(48), Display(Name = "Home Phone")]
    public string? HomePhone { get; set; }

    [StringLength(48)] public string? ImportedName { get; set; }

    [StringLength(48), Display(Name = "Last Name")]
    public string? LastName { get; set; }


    [StringLength(48), Display(Name = "Middle Name")]
    public string? MiddleName { get; set; }


    public DateTime? ModifyDate { get; set; }

    [StringLength(48), Display(Name = "Name Prefix")]
    public string? NamePrefix { get; set; }


    [StringLength(48), Display(Name = "Name Suffix")]
    public string? NameSuffix { get; set; }


    [StringLength(48), Display(Name = "Non Employee ID")]
    public string? NonEmployeeID { get; set; }


    [StringLength(48), Display(Name = "Other Phone")]
    public string? OtherPhone { get; set; }


    [StringLength(48), Display(Name = "Primary Address")]
    public string? PrimaryAddress { get; set; }


    public int? PrimaryEmail { get; set; }
    public int PrimaryFinancialAccount { get; set; }
    public bool ScofflawFlag { get; set; }


    [StringLength(48)]
    public string? Subclassification { get; set; }


    [StringLength(48), Display(Name = "Tertiary ID")]
    public string? TertiaryID { get; set; }


    [StringLength(48), Display(Name = "Work Phone")]
    public string? WorkPhone { get; set; }



    [JsonIgnore, AuditIgnore]
    [Timestamp]
    public byte[] timestamp { get; set; }

    public virtual ICollection<Permit> Permits { get; set; }
    public virtual ICollection<Email> Emails { get; set; }
    public virtual ICollection<CustomerNote> Notes { get; set; }


    public Customer()
    {
      Permits = new HashSet<Permit>();
      Emails = new HashSet<Email>();
      Notes = new HashSet<CustomerNote>();
    }
    public int GetId()
    {
      return CustomerUID;
    }
  }
}
