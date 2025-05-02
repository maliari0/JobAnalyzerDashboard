using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using JobAnalyzerDashboard.Server.Data;
using Microsoft.EntityFrameworkCore;

namespace JobAnalyzerDashboard.Server.Services
{
    public class EmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly GmailApiService _gmailService;
        private readonly ApplicationDbContext _context;

        public EmailService(ILogger<EmailService> logger, GmailApiService gmailService, ApplicationDbContext context)
        {
            _logger = logger;
            _gmailService = gmailService;
            _context = context;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, string? attachmentPath = null, int profileId = 1)
        {
            try
            {
                // Kullanıcının OAuth token'ı var mı kontrol et
                var oauthToken = await _context.OAuthTokens
                    .FirstOrDefaultAsync(t => t.ProfileId == profileId);

                if (oauthToken != null)
                {
                    // OAuth ile e-posta gönder
                    return await _gmailService.SendEmailAsync(profileId, to, subject, body, attachmentPath);
                }
                else
                {
                    // Varsayılan SMTP ile e-posta gönder
                    using var client = new SmtpClient
                    {
                        Host = "smtp.protonmail.ch",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential("aliari0@proton.me", "102030NNmm")
                    };

                    var message = new MailMessage
                    {
                        From = new MailAddress("aliari0@proton.me", "Job Analyzer Dashboard"),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    message.To.Add(new MailAddress(to));

                    if (!string.IsNullOrEmpty(attachmentPath))
                    {
                        message.Attachments.Add(new Attachment(attachmentPath));
                    }

                    await client.SendMailAsync(message);

                    _logger.LogInformation("E-posta gönderildi: {To}, Konu: {Subject}", to, subject);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta gönderilirken hata oluştu: {To}, Konu: {Subject}", to, subject);
                return false;
            }
        }
    }
}
