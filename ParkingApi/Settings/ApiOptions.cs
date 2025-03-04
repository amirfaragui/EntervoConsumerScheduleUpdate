using System.ComponentModel.DataAnnotations;

namespace Entrvo.Api
{
  public class ApiOptions
  {
    public string Server { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int? ContractNumber { get; set; }
    public int? InstanceNumber { get; set; }
  }
}
