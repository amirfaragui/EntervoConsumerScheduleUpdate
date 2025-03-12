using Newtonsoft.Json;
using System.Xml.Serialization;

namespace Entrvo.Api.Models
{
  public class UsageProfile
  {
    [JsonProperty("@href")]
    [XmlElement("href")]
    public string Href { get; set; }
    [XmlElement("id")]
    public int Id { get; set; }
    [XmlElement("name")]
    public string Name { get; set; }
    [XmlElement("description")]
    public string Description { get; set; }

    public override string ToString()
    {
      return $"{Id} - {Name}";
    }
  }


  class ProfileList 
  {
    [JsonProperty("usageProfile")]
    public UsageProfile[] Profiles { get; set; }

  }

  class ProfileListResponse
  {
    [JsonProperty("usageProfiles")]
    public ProfileList Result { get; set; }
  }
}
