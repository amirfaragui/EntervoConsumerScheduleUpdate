using System.ComponentModel.DataAnnotations;

namespace T2WebApplication.Models
{
  public class ColumnMappingModel
  {
    public int Index { get; set; }
    public string Value { get; set; }
    public string Header { get; set; }
    public string Format { get; set; }
    public string Target { get; set; }
    public string Name
    {
      get
      {
        if (string.IsNullOrWhiteSpace(Header)) return $"Column# {Index}";
        return Header;
      }
    }
  }

  //public enum ColumnMapping
  //{
  //  NotUsed,
  //  CardNumber,
  //  CardType,
  //  Active,
  //  FirstName,
  //  LastName,
  //  FullName,
  //  Email,
  //  Telephone,
  //  Comments,
  //  StartValidity,
  //  EndValidity,
  //  Group,
  //  Company,
  //  Term,
  //}
}
