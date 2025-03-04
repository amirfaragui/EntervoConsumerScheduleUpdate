namespace Entrvo.Services.Models
{
  public class EmailOptions
  {
    public string MailServer { get; set; }
    public int MailPort { get; set; }
    public string SenderName { get; set; }
    public string Sender { get; set; }
    public string Password { get; set; }
    public bool UseSSL { get; set; }
    public bool UseTLS { get; set; }

    public string[] Recipients { get; set; }

    public EmailOptions()
    {
      Recipients = new string[0];
    }
  }
}
