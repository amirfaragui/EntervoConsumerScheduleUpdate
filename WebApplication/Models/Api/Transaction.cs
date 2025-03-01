using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SnB.Models
{
  public class Transaction
  {
    [XmlElement("shiftId", Namespace = "http://gsph.sub.com/payment/types")]
    public string ShiftId { get; set; }

    [XmlElement("computerId", Namespace = "http://gsph.sub.com/payment/types")] 
    public string ComputerId { get; set; }

    [XmlElement("deviceId", Namespace = "http://gsph.sub.com/payment/types")] 
    public string DeviceId { get; set; }

    [XmlElement("cashierContractId", Namespace = "http://gsph.sub.com/payment/types")] 
    public string CashierContractId { get; set; }

    [XmlElement("cashierConsumerId", Namespace = "http://gsph.sub.com/payment/types")] 
    public string CashierConsumerId { get; set; }

    [XmlElement("salesTransactionId", Namespace = "http://gsph.sub.com/payment/types")] 
    public string Id { get; set; }

    [XmlElement("salesTransactionDateTime", Namespace = "http://gsph.sub.com/payment/types")] 
    public DateTime TimeCreated { get; set; }

    public Transaction()
    {
      Id = DateTime.Now.ToString("yyMMddHHmmss");
      TimeCreated = DateTime.Now;
    }
  }

  public class MoneyValueCard
  {
    [XmlElement("ContractId", Namespace = "http://gsph.sub.com/payment/types")]
    public string ContractId { get; set; }
    [XmlElement("ConsumerId", Namespace = "http://gsph.sub.com/payment/types")]
    public string ConsumerId { get; set; }

    [XmlElement("addMoneyValue", Namespace = "http://gsph.sub.com/payment/types")]
    public int amount { get; set; }

    public MoneyValueCard()
    {

    }

    public MoneyValueCard(decimal value)
    {
      amount = Convert.ToInt32(value * 100);
    }
  }

  public class Article
  {
    [XmlElement("artClassRef", Namespace = "http://gsph.sub.com/payment/types")]
    public string artClassRef { get; set; } = "0";

    [XmlElement("articleRef", Namespace = "http://gsph.sub.com/payment/types")]
    public string articleRef { get; set; } = "10624";

    [XmlElement("quantity", Namespace = "http://gsph.sub.com/payment/types")]
    public int quantity { get; set; } = 1;

    [XmlElement("quantityExp", Namespace = "http://gsph.sub.com/payment/types")]
    public int quantityExp { get; set; } = 0;

    [XmlElement("amount", Namespace = "http://gsph.sub.com/payment/types")]
    public int amount { get; set; }

    [XmlElement("influenceRevenue", Namespace = "http://gsph.sub.com/payment/types")]
    public int influenceRevenue { get; set; } = 1;

    [XmlElement("influenceCashFlow", Namespace = "http://gsph.sub.com/payment/types")]
    public int influenceCashFlow { get; set; } = 1;

    [XmlElement("personalizedMoneyValueCard", Namespace = "http://gsph.sub.com/payment/types")]
    public MoneyValueCard personalizedMoneyValueCard { get; set; }
    
  }

  [XmlRoot("salesTransactionDetail", Namespace = "http://gsph.sub.com/payment/types")]
  public class TransactionDetail
  {
    [XmlElement("salesTransaction", Namespace = "http://gsph.sub.com/payment/types")]
    public Transaction Transaction { get; set; }

    [XmlArray("articles", Namespace = "http://gsph.sub.com/payment/types")]
    [XmlArrayItem("article", Namespace = "http://gsph.sub.com/payment/types")]
    public Article[] Articles { get; set; }

    public TransactionDetail(MoneyValueCard moneyValueCard)
    {
      var article = new Article
      {
        personalizedMoneyValueCard = moneyValueCard,
        amount = 0  //moneyValueCard.amount,
      };

      Articles = new Article[] { article };
    }

    public TransactionDetail()
    {

    }
  }
}
