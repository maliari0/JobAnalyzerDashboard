using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;

        public EmailService(
            ILogger<EmailService> logger,
            GmailApiService gmailService,
            ApplicationDbContext context,
            IConfiguration configuration)
        {
            _logger = logger;
            _gmailService = gmailService;
            _context = context;
            _configuration = configuration;
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
                    // E-posta ayarlarını yapılandırmadan al
                    var emailSettings = _configuration.GetSection("EmailSettings");
                    var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
                    var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                    var smtpUsername = emailSettings["SmtpUsername"] ?? "aliari.test@gmail.com";
                    var smtpPassword = emailSettings["SmtpPassword"] ?? "";
                    var senderEmail = emailSettings["SenderEmail"] ?? "aliari.test@gmail.com";
                    var senderName = emailSettings["SenderName"] ?? "Job Analyzer Dashboard";

                    // Gmail SMTP ile e-posta gönder
                    using var client = new SmtpClient
                    {
                        Host = smtpServer,
                        Port = smtpPort,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(smtpUsername, smtpPassword)
                    };

                    var message = new MailMessage
                    {
                        From = new MailAddress(senderEmail, senderName),
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
