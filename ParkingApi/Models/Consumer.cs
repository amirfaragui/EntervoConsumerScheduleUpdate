using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Xml.Serialization;

namespace Entrvo.Api.Models
{
  public class Consumer
  {
    [XmlAttribute("href")]
    [JsonProperty("@href")]
    public string Href { get; set; }


    [XmlElement("id")]
    public string? Id { get; set; }


    [JsonProperty("contractid")]
    [XmlElement("contractid")]
    public string ContractId { get; set; }

    [XmlElement("name")]
    public string? Name { get; set; }


    [JsonProperty("xValidFrom")]
    [XmlElement("xValidFrom")]
    public string? ValidFrom { get; set; }


    [JsonProperty("xValidUntil")]
    [XmlElement("xValidUntil")]
    public string? ValidUntil { get; set; }
    
    
    [XmlElement("filialId")]
    public string? FilialId { get; set; }
  }

  public class Person
  {
    [XmlElement("firstName")]
    public string? FirstName { get; set; }
    [XmlElement("surname")]
    public string? Surname { get; set; }
    [XmlElement("matchCode")]
    public string? MatchCode { get; set; }
  }

  public class Identification
  {
    [XmlElement("ptcptType")]
    public int PtcptType { get; set; }
    public int Cardclass { get; set; }
    [JsonProperty("cardno")]
    [XmlElement("cardno")]
    public string CardNumber { get; set; }
    [XmlElement("identificationType")]
    public int IdentificationType { get; set; }
    [XmlElement("validFrom")]
    public string? ValidFrom { get; set; }
    [XmlElement("validUntil")]
    public string? ValidUntil { get; set; }

    [XmlElement("usageProfile")]
    public UsageProfile UsageProfile { get; set; }
    [XmlElement("admission")]
    public string Admission { get; set; }
    [XmlElement("ignorePresence")]
    public int IgnorePresence { get; set; }

    [JsonConverter(typeof(BooleanJsonConverter))]
    [XmlElement("present")]
    public bool Present { get; set; }
    [XmlElement("status")]
    public int Status { get; set; }
    [XmlElement("ptcpGrpNo")]
    public int PtcpGrpNo { get; set; }
    [JsonProperty("ChrgOvdrftAcct")]
    [XmlElement("chrgOvdrftAcct")]
    public int ChargeOverDraftAccount { get; set; }
  }

  public class ConsumerAttributes
  {
    [XmlElement("productionDate")]
    public string ProductionDate { get; set; }
    [XmlElement("productionCount")]
    public int ProductionCount { get; set; }
    [XmlElement("flatFeeFirstCharge")]
    public int FlatFeeFirstCharge { get; set; }
    [XmlElement("flatFeeLastCharge")]
    public int FlatFeeLastCharge { get; set; }
    [XmlElement("flatFeeCalc")]
    public int FlatFeeCalc { get; set; }
    [XmlElement("flatFeeCalcUntil")]
    public string FlatFeeCalcUntil { get; set; }
    [XmlElement("individualInvoicing")]
    public int IndividualInvoicing { get; set; }
    [XmlElement("flatRateAmt")]
    public int FlatRateAmt { get; set; }
    [XmlElement("flatFeeTax")]
    public string FlatFeeTax { get; set; }
    [XmlElement("invoiceType")]
    public string InvoiceType { get; set; }
  }


  [XmlRoot("consumerDetail", Namespace = "http://gsph.sub.com/payment/types")]
  public class ConsumerDetails
  {
    [XmlElement("consumer")]
    public Consumer Consumer { get; set; }
    [XmlElement("person")]
    public Person Person { get; set; }
    [XmlElement("firstName")]
    public string? FirstName { get; set; }
    [XmlElement("surname")]
    public string? Surname { get; set; }
    [XmlElement("identification")] 
    public Identification Identification { get; set; }
    [JsonProperty("ConsumerAtributes")]
    [XmlElement("consumerAtributes")]
    public ConsumerAttributes ConsumerAttributes { get; set; }
    [XmlElement("displayText")]
    public string? DisplayText { get; set; }
    [XmlElement("limit")]
    public int Limit { get; set; }
    [XmlElement("status")]
    public int Status { get; set; }
    [XmlElement("delete")]
    public int Delete { get; set; }
    [XmlElement("ignorePresence")]
    public int IgnorePresence { get; set; }
    [XmlElement("lpn1")]
    public string? Lpn1 { get; set; }
    [XmlElement("lpn2")]
    public string? Lpn2 { get; set; }
    [XmlElement("lpn3")]
    public string? Lpn3 { get; set; }

    [JsonProperty("userfield1")]
    [XmlElement("userfield1")]
    public string? UserField1 { get; set; }
    [JsonProperty("userfield2")]
    [XmlElement("userfield2")]
    public string? UserField2 { get; set; }
    [JsonProperty("userfield3")]
    [XmlElement("userfield3")]
    public string? UserField3 { get; set; }

    [XmlElement("memo")]
    public string? Memo { get; set; }

    public override string ToString()
    {
      return $"Name= '{FirstName} {Surname}', ContractId= '{Consumer?.ContractId}', ConsumerId= '{Consumer?.Id}', MatchCode='{Person.MatchCode}'";
    }

    public string CardNumber
    {
      get
      {
        return Identification?.CardNumber ?? $"{Consumer.ContractId},{Consumer.Id}";
      }
    }
  }


  interface IConsumers
  {
    Consumer[] Consumers { get; }
  }

  interface IConsumersProvider
  {
    IConsumers Consumers { get; }
  }

  class ConsumerList: IConsumers
  {
    [JsonProperty("consumer")]
    public Consumer[] Consumers { get; set; }

  }


  class SingleConsumer: IConsumers
  {  
    public Consumer Consumer { get; set; }

    [JsonIgnore]
    public Consumer[] Consumers => new Consumer[] { Consumer };
  }

  class ConsumerListResponse: IConsumersProvider
  {    
    public ConsumerList Consumers { get; set; }

    IConsumers IConsumersProvider.Consumers => Consumers;
  }

  class SingleConsumerResponse: IConsumersProvider
  {
    public SingleConsumer Consumers { get; set; }

    IConsumers IConsumersProvider.Consumers => Consumers;
  }

  class ConsumerDetailResponse
  {
    public ConsumerDetails ConsumerDetail { get; set; }
  }


  class BalanceResponse
  {
    public string Epan { get; set; }
    public decimal? MoneyValue { get; set; }
  }
}
