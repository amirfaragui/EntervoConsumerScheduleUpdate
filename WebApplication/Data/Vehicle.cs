using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Entrvo.DAL.Attributes;

namespace Entrvo.DAL
{
  public class Vehicle: IDataEntity
  {
    [Column(TypeName = "decimal(7,2)")] public decimal AmountDue { get; set; }
    public bool HasHandheldNotifications { get; set; }
    [StringLength(20)] public string? LicensePlate { get; set; }
    public DateTime? ModifyDate { get; set; }
    [StringLength(20)] public string? OfficialRegistrationinformationfromDMV { get; set; }
    [StringLength(20)] public string? PlateRegExpMonth { get; set; }
    [StringLength(20)] public string? PlateRegExpYear { get; set; }
    [StringLength(20)] public string? PlateSeries { get; set; }
    [StringLength(20)] public string? PlateType { get; set; }
    [StringLength(20)] public string? Province { get; set; }
    public bool RoVREligible { get; set; }
    public DateTime? RoVRLastSent { get; set; }
    public DateTime? RoVRLastUpdated { get; set; }
    public bool ScofflawFlag { get; set; }
    public bool SelectedforNextRoVR { get; set; }
    public int SeriesEndDate { get; set; }
    public int SeriesStartDate { get; set; }
    [StringLength(20)] public string? VehicleColor { get; set; }
    [StringLength(20)] public string? VehicleMake { get; set; }
    [StringLength(20)] public string? VehicleModel { get; set; }
    [StringLength(20)] public string? VehicleStyle { get; set; }
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int VehicleUID { get; set; }
    [StringLength(20)] public string? Vin { get; set; }
    [StringLength(20)] public string? Year { get; set; }

    [JsonIgnore, AuditIgnore]
    [Timestamp]
    public byte[] timestamp { get; set; }

    public int GetId()
    {
      return VehicleUID;
    }
  }
}
