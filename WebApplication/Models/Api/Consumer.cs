using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace SnB.Models
{

  public class ConsumerModel
  {
    [Editable(false)]
    public string Id { get; set; }
    [Editable(false)] 
    public string FirstName { get; set; }
    [Editable(false)] 
    public string Surname { get; set; }
    [Editable(false)] 
    public string ValidUntil { get; set; }
    [Editable(false)] 
    public string CardNumber { get; set; }
    [Editable(false)] 
    public decimal? Balance { get; set; }
  }

  public class ConsumerTopupModel : ConsumerModel
  {
    [Required]
    [Range(0.01, 1000)]
    public decimal Amount { get; set; }

    public ConsumerTopupModel()
    {
      Amount = 1m;
    }
  }

  class BalanceResponse
  {
    public string Epan { get; set; }
    public decimal? MoneyValue { get; set; }
  }
}
