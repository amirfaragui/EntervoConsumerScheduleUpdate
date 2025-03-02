using Newtonsoft.Json;

namespace Entrvo.Api.Models
{
  public class Cashier
  {
    public string CashierContractId { get; set; }
    public string CashierConsumerId { get; set; }
    public string Surname { get; set; }
  }

  class CashierListResponse
  {
    [JsonProperty("cashier")]
    public Cashier[] Cashiers { get; set; }

  }
}
