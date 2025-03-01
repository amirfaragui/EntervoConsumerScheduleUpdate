using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using T2Importer.DAL.Attributes;

namespace T2Importer.DAL
{
  public class Permit : IDataEntity
  {
    [StringLength(48), Display(Name = "Group")] public string? AccessGroup { get; set; }
    [StringLength(48)] public string? ActiveCredentialID { get; set; }
    public int AddedValue { get; set; }
    public DateTime? AllocateReturnDate { get; set; }
    public DateTime? AllocateStartDate { get; set; }
    public int? Allocator { get; set; }
    [StringLength(48)] public string? AnonymousUserID { get; set; }
    [StringLength(48)] public string? BulkPermit { get; set; }
    [StringLength(48)] public string? CardMode { get; set; }
    [StringLength(256)] public string? Comment { get; set; }
    public int? ControlGroup { get; set; }
    public bool Current { get; set; }
    [StringLength(48)] public string? Custody { get; set; }
    [StringLength(48)] public string? CustodyType { get; set; }
    [StringLength(48)] public string? DeactivateReason { get; set; }
    public DateTime? DeactivatedDate { get; set; }
    [StringLength(48)] public string? Deallocator { get; set; }
    [Column(TypeName = "decimal(7,2)"), Display(Name = "Deposit Paid")] public decimal? DepositAmountPaid { get; set; }
    [Column(TypeName = "decimal(7,2)"), Display(Name = "Deposit Fee")] public decimal? DepositFee { get; set; }
    [StringLength(48)] public string? Drawer { get; set; }
    [Display(Name = "Effective Date")] public DateTime? EffectiveDate { get; set; }
    [StringLength(48)] public string? EmailNotificationAddress { get; set; }
    [Display(Name = "Expiration Date")] public DateTime? ExpirationDate { get; set; }
    public bool ExporttoWPSRequired { get; set; }
    public int? FeeSchedAtPurch { get; set; }
    public bool HasDepositbeenrefunded { get; set; }
    public bool HasLetter { get; set; }
    public bool HasNote { get; set; }
    public bool HasPendingLetter { get; set; }
    [Display(Name = "Deactivated")] public bool IsDeactivated { get; set; }
    public bool IsDestroyed { get; set; }
    public bool IsEmailNotifyReq { get; set; }
    public bool IsFulfilling { get; set; }
    public bool IsHistorical { get; set; }
    public bool IsMailingReq { get; set; }
    [Display(Name = "Missing")] public bool IsMissing { get; set; }
    public bool IsMissingfromCustody { get; set; }
    public bool IsPermitDirectFulfilled { get; set; }
    public bool IsPossConfReq { get; set; }
    public bool IsReturned { get; set; }
    public bool IsReturningtoMainInventory { get; set; }
    [Display(Name = "Terminated")] public bool IsTerminated { get; set; }
    [Display(Name = "Issue Number"), Range(1, int.MaxValue)] 
    public int IssueNumber { get; set; }
    public DateTime? LaneControllerStampDate { get; set; }
    public DateTime? LastExportedtoWPS { get; set; }
    [StringLength(48)] public string? MailTrackingNumber { get; set; }
    [StringLength(48)] public string? MailingAddress { get; set; }
    public DateTime? MailingDate { get; set; }
    [StringLength(48)] public string? MaximumValue { get; set; }
    public DateTime? MissingDate { get; set; }
    [StringLength(48)] public string? MissingReason { get; set; }
    public DateTime? ModifyDate { get; set; }
    public int PemissionRawNumber { get; set; }
    public DateTime? PendingExpirationDate { get; set; }
    [StringLength(48)] public string? PermitAllotment { get; set; }
    [Column(TypeName = "decimal(7,2)"), Display(Name = "Amount Due")] public decimal PermitAmountDue { get; set; }
    [StringLength(48), Display(Name = "Permit Number")] public string? PermitNumber { get; set; }
    [StringLength(48)] public string? PermitNumberRange { get; set; }
    [StringLength(48)] public string? PermitSeriesType { get; set; }
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Display(Name = "Permit ID")]
    public int PermitUID { get; set; }
    [StringLength(48)] public string? PermitDirectStatus { get; set; }
    [StringLength(48)] public string? PhysicalGroupType { get; set; }
    public DateTime? PossessionDate { get; set; }
    [ForeignKey("Customer"), Display(Name = "Customer ID")]
    public int? PurchasingCustomer { get; set; }
    [StringLength(48)] public string? PurchasingThirdParty { get; set; }
    public DateTime? RenewalGracePeriodEndingDate { get; set; }
    [StringLength(48)] public string? RenewalStatus { get; set; }
    public int? RenewalUID { get; set; }
    [StringLength(48), Display(Name = "Replacement#")] public string? ReplacementPermit { get; set; }
    public DateTime? ReservationNoShowCutoffDate { get; set; }
    public DateTime? ReserveEndDate { get; set; }
    [StringLength(48)] public string? ReserveHold { get; set; }
    public DateTime? ReserveStartDate { get; set; }
    [StringLength(48)] public string? Reserved { get; set; }
    [StringLength(48)] public string? ReturnDate { get; set; }
    [StringLength(48)] public string? ReturnReason { get; set; }
    [StringLength(48)] public string? ShippingMethod { get; set; }
    public DateTime? SoldDate { get; set; }
    [StringLength(48)] public string? StallId { get; set; }
    [StringLength(48)] public string? StallType { get; set; }
    [StringLength(48)] public string? Status { get; set; }
    public DateTime? TerminatedDate { get; set; }
    [StringLength(48), Display(Name = "Status")] public string? WorkflowStatus { get; set; }

    public virtual Customer Customer { get; set; }

    public virtual ICollection<PermitNote> Notes { get; set; }

    [JsonIgnore, AuditIgnore]
    [Timestamp]
    public byte[] timestamp { get; set; }


    public Permit()
    {
      Notes = new HashSet<PermitNote>();
      IssueNumber = 1;
      ModifyDate = DateTime.Now;
    }

    public int GetId()
    {
      return PermitUID;
    }
  }

  public class PermitWithCustomerName : Permit
  {
    [NotMapped]
    public string CustomerName { get; set; }
  }
}
