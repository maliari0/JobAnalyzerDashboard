using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;

        public EmailService(ILogger<EmailService> logger)
        {
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, string attachmentPath = null)
        {
            try
            {
                using (var client = new SmtpClient())
                {
                    client.Host = "smtp.protonmail.ch";
                    client.Port = 587;
                    client.EnableSsl = true;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential("aliari0@proton.me", "102030NNmm");

                    var message = new MailMessage();
                    message.From = new MailAddress("aliari0@proton.me", "Job Analyzer Dashboard");
                    message.To.Add(new MailAddress(to));
                    message.Subject = subject;
                    message.Body = body;
                    message.IsBodyHtml = true;

                    if (!string.IsNullOrEmpty(attachmentPath))
                    {
                        message.Attachments.Add(new Attachment(attachmentPath));
                    }

                    await client.SendMailAsync(message);

                    _logger.LogInformation("E-posta gönderildi: {to}, Konu: {subject}", to, subject);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilirken hata oluştu: {to}, Konu: {subject}", to, subject);
                return false;
            }
        }
    }
}
