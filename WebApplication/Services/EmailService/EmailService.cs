using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Utils;
using System.Drawing;
using System.Drawing.Imaging;
using Entrvo.Services.Models;

namespace Entrvo.Services
{
  public class EmailService : IEmailSender, IEmailService
  {
    private readonly EmailOptions _emailSettings;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IOptions<EmailOptions> emailSettings,
        IWebHostEnvironment env,
        ILogger<EmailService> logger)
    {
      _emailSettings = emailSettings.Value;
      _env = env;
      _logger = logger;
    }

    public async Task SendEmailAsync(string recipient, string subject, string message)
    {
      try
      {
        var mimeMessage = new MimeMessage();

        mimeMessage.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.Sender));

        mimeMessage.To.Add(MailboxAddress.Parse(recipient));

        mimeMessage.Subject = subject;

        mimeMessage.Body = new TextPart("html")
        {
          Text = message
        };

        using (var client = await CreateSmtpClient())
        {
          await client.SendAsync(mimeMessage);

          await client.DisconnectAsync(true);
        }
      }
      catch (Exception ex)
      {
        // TODO: handle exception
        _logger.LogError(ex.ToString());
        throw new InvalidOperationException(ex.Message);
      }
    }

    public Task SendEmailAsync(string subject, string htmlBody, params object[] attachments)
    {
      return SendEmailAsync(_emailSettings.Recipients, _emailSettings.Sender, subject, htmlBody, attachments);
    }

    public async Task SendEmailAsync(string[] recipients, string senderName, string subject, string htmlBody, params object[] attachments)
    {
      try
      {
        var mimeMessage = new MimeMessage();

        mimeMessage.From.Add(new MailboxAddress(senderName, _emailSettings.Sender));

        if (recipients.Count() == 1)
        {
          mimeMessage.To.Add(MailboxAddress.Parse(recipients[0]));
        }
        else
        {
          foreach(var r in recipients)
          {
            mimeMessage.Bcc.Add(MailboxAddress.Parse(r));
          }
        }

        mimeMessage.Subject = subject;

        var builder = new BodyBuilder();

        //var arguments = new List<string>();

        int index = 0;
        foreach (var attachment in attachments)
        {
          if (attachment is Bitmap bitmap)
          {
            using (var stream = new MemoryStream())
            {
              bitmap.Save(stream, ImageFormat.Jpeg);
              var image = builder.LinkedResources.Add($"image_{index + 1}.jpg", stream.ToArray());
              image.ContentId = MimeUtils.GenerateMessageId();
              //arguments.Add(image.ContentId);

              htmlBody = htmlBody.Replace($"cid:{{{index}}}", $"cid:{image.ContentId}");
            }
          }
          else
          {
            var fileName = attachment.ToString();
            if (File.Exists(fileName))
            {
              builder.Attachments.Add(fileName);
            }
          }
          index++;
        }

        builder.HtmlBody = htmlBody;


        mimeMessage.Body = builder.ToMessageBody();

        using (var client = await CreateSmtpClient())
        {
          await client.SendAsync(mimeMessage);

          await client.DisconnectAsync(true);
        }

      }
      catch (Exception ex)
      {
        _logger.LogError(ex.ToString());
        throw new InvalidOperationException(ex.Message);
      }
    }

    private async Task<SmtpClient> CreateSmtpClient()
    {
      var client = new SmtpClient();

      client.ServerCertificateValidationCallback = (s, c, h, e) => true;

      if (_emailSettings.UseSSL)
      {
        await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort, _emailSettings.UseSSL);
      }
      else if (_emailSettings.UseTLS)
      {
        await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort, MailKit.Security.SecureSocketOptions.StartTls);
      }
      else
      {
        await client.ConnectAsync(_emailSettings.MailServer, _emailSettings.MailPort);
      }
      if (!string.IsNullOrEmpty(_emailSettings.Password))
      {
        await client.AuthenticateAsync(_emailSettings.Sender, _emailSettings.Password);
      }

      return client;
    }
  }

}
