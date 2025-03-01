using System.Threading.Tasks;

namespace T2WebApplication.Services
{
  public interface IEmailService
  {
    Task SendEmailAsync(string[] recipients, string senderName, string subject, string htmlBody, params object[] attachments);
    Task SendEmailAsync(string subject, string htmlBody, params object[] attachments);
  }
}
