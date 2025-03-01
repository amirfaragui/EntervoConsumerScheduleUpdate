using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SnB.Models
{
  public class Shift
  {
    public string ShiftId { get; set; }
    public string ComputerId { get; set; }
    public string DeviceId { get; set; }
    public string CashierContractId { get; set; }
    public string CashierConsumerId { get; set; }
    public string ShiftNo { get; set; }
    public DateTimeOffset? CreateDateTime { get; set; }
    public DateTimeOffset? FinishDateTime { get; set; }

    public int ShiftStatus { get; set; }
    public int LastSalesTransactionId { get; set; }
  }

  class ShiftResponse
  {    
    public Shift Shift { get; set; }
  }

  class ShiftListResponse
  {
    [JsonProperty("shift")]
    public Shift[] Shifts { get; set; }
  }


  [XmlRoot("shift", Namespace = "http://gsph.sub.com/payment/types")]
  public class NewShift
  {
    [XmlElement("computerId", Namespace = "http://gsph.sub.com/payment/types")]
    public string ComputerId { get; set; }

    [XmlElement("deviceId", Namespace = "http://gsph.sub.com/payment/types")]
    public string DeviceId { get; set; }

    [XmlElement("cashierContractId", Namespace = "http://gsph.sub.com/payment/types")]
    public string CashierContractId { get; set; }

    [XmlElement("cashierConsumerId", Namespace = "http://gsph.sub.com/payment/types")]
    public string CashierConsumerId { get; set; }

    [XmlElement("shiftNo", Namespace = "http://gsph.sub.com/payment/types")]
    public string ShiftNo { get; set; }

    [XmlElement("createDateTime", Namespace = "http://gsph.sub.com/payment/types")]
    public DateTime CreateDateTime { get; set; }
  }

  public class NewShiftResponse: Shift
  {
    [JsonProperty("@href")]
    public string Href { get; set; }
  }
}
