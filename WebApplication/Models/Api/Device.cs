using Newtonsoft.Json;

namespace SnB.Models
{
  public class Device
  {
    public string ComputerId { get; set; }
    public string DeviceId { get; set; }
    public string DeviceShortName { get; set; }
    public string DeviceLongName { get; set; }
  }

  class DeviceListResponse
  {
    [JsonProperty("device")]
    public Device[] Devices { get; set; }
  }
}
