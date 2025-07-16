using Entrvo;

namespace EntrvoWebApp.Services.Models
{
  public class EntrvoRecord : IDataEntity<string?>
  {
    public string? Status { get; set; }
    public string CardNumber { get; set; }
    public string? AccessProfile { get; set; }

    public string? ColumnD { get; set; }

    public string? ColumnE { get; set; }

    public DateTime? StartValidity { get; set; }

    public string? ColumnG { get; set; }

    public DateTime? EndValidity { get; set; }
    public string? AdditionalValue { get; set; }
    public string? ColumnJ { get; set; }
    public string? LastName { get; set; }
    public string? FirstName { get; set; }
    public string? ColumnM { get; set; }
    public string? ColumnN { get; set; }
    public string? ColumnO { get; set; }
    public string? Note1 { get; set; }
    public string? Note1Extended { get; set; }
    public string? LPN1 { get; set; }
    public string? LPN2 { get; set; }
    public string? ColumnT { get; set; }
    public string? Note2 { get; set; }
    public string? Note3 { get; set; }
    public string? ColumnW { get; set; }
    public string? ColumnX { get; set; }


    public string? GetId()
    {
      if (string.IsNullOrEmpty(CardNumber)) return null;
      var cardNumberString = CardNumber.Trim();
      //var siteCode = cardNumberString?[..3];
      var siteCode = cardNumberString.Substring(0, cardNumberString.Length - 5);
      //      var cardNumber = cardNumberString?[3..] ?? string.Empty;
      var cardNumber = cardNumberString.Substring(siteCode.Length) ;

      return siteCode + cardNumber.PadLeft(8, '0');
    }

     public override string ToString()
    {
      return $"{Status},{CardNumber},{AccessProfile},{StartValidity},{EndValidity},{AdditionalValue},{LastName},{FirstName},{Note1},{Note1Extended},{LPN1},{LPN2},{Note2},{Note3}";
    }
  }
}
