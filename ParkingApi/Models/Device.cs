using Newtonsoft.Json;

namespace Entrvo.Api.Models
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
