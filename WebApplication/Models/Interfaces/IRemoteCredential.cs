namespace Entrvo.Models
{
  public interface IRemoteCredential
  {
    string Server { get; set; }
    string UserName { get; set; }
    string Password { get; set; }
  }
}
