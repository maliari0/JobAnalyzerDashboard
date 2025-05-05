using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using JobAnalyzerDashboard.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

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
                Debug.WriteLine($"E-posta gönderme isteği alındı: {to}, Konu: {subject}");
                Console.WriteLine($"E-posta gönderme isteği alındı: {to}, Konu: {subject}");
                _logger.LogInformation("E-posta gönderme isteği alındı: {To}, Konu: {Subject}", to, subject);

                // Doğrudan SMTP kullanarak e-posta gönder
                Debug.WriteLine("Doğrudan SMTP kullanarak e-posta gönderiliyor");
                Console.WriteLine("Doğrudan SMTP kullanarak e-posta gönderiliyor");

                // E-posta ayarlarını yapılandırmadan al
                var emailSettings = _configuration.GetSection("EmailSettings");
                var smtpServer = emailSettings["SmtpServer"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(emailSettings["SmtpPort"] ?? "587");
                var smtpUsername = emailSettings["SmtpUsername"] ?? "aliari.test@gmail.com";
                var smtpPassword = emailSettings["SmtpPassword"] ?? "";
                var senderEmail = emailSettings["SenderEmail"] ?? "aliari.test@gmail.com";
                var senderName = emailSettings["SenderName"] ?? "Job Analyzer Dashboard";

                Debug.WriteLine($"SMTP Ayarları: Server={smtpServer}, Port={smtpPort}, Username={smtpUsername}, SenderEmail={senderEmail}");
                Console.WriteLine($"SMTP Ayarları: Server={smtpServer}, Port={smtpPort}, Username={smtpUsername}, SenderEmail={senderEmail}");
                _logger.LogInformation("SMTP Ayarları: Server={Server}, Port={Port}, Username={Username}, SenderEmail={SenderEmail}",
                    smtpServer, smtpPort, smtpUsername, senderEmail);

                // Gmail SMTP ile e-posta gönder
                using var client = new SmtpClient
                {
                    Host = smtpServer,
                    Port = smtpPort,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                    Timeout = 30000 // 30 saniye timeout
                };

                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(to));

                Debug.WriteLine($"MailMessage oluşturuldu: From={message.From}, To={message.To}, Subject={message.Subject}");
                Console.WriteLine($"MailMessage oluşturuldu: From={message.From}, To={message.To}, Subject={message.Subject}");
                _logger.LogInformation("MailMessage oluşturuldu: From={From}, To={To}, Subject={Subject}",
                    message.From, message.To, message.Subject);

                if (!string.IsNullOrEmpty(attachmentPath))
                {
                    if (System.IO.File.Exists(attachmentPath))
                    {
                        message.Attachments.Add(new Attachment(attachmentPath));
                        Debug.WriteLine($"Dosya eklendi: {attachmentPath}");
                        Console.WriteLine($"Dosya eklendi: {attachmentPath}");
                        _logger.LogInformation("Dosya eklendi: {AttachmentPath}", attachmentPath);
                    }
                    else
                    {
                        Debug.WriteLine($"Ek dosyası bulunamadı: {attachmentPath}");
                        Console.WriteLine($"Ek dosyası bulunamadı: {attachmentPath}");
                        _logger.LogWarning("Ek dosyası bulunamadı: {AttachmentPath}", attachmentPath);
                    }
                }

                Debug.WriteLine("E-posta gönderiliyor...");
                Console.WriteLine("E-posta gönderiliyor...");
                _logger.LogInformation("E-posta gönderiliyor...");

                try
                {
                    await client.SendMailAsync(message);
                    Debug.WriteLine($"E-posta başarıyla gönderildi: {to}, Konu: {subject}");
                    Console.WriteLine($"E-posta başarıyla gönderildi: {to}, Konu: {subject}");
                    _logger.LogInformation("E-posta başarıyla gönderildi: {To}, Konu: {Subject}", to, subject);
                    return true;
                }
                catch (Exception smtpEx)
                {
                    Debug.WriteLine($"SMTP hatası: {smtpEx.Message}");
                    Console.WriteLine($"SMTP hatası: {smtpEx.Message}");
                    _logger.LogError(smtpEx, "SMTP hatası: {ErrorMessage}", smtpEx.Message);

                    // İç içe istisnalar varsa onları da logla
                    var innerException = smtpEx.InnerException;
                    int level = 1;
                    while (innerException != null)
                    {
                        Debug.WriteLine($"İç istisna {level}: {innerException.Message}");
                        Console.WriteLine($"İç istisna {level}: {innerException.Message}");
                        _logger.LogError(innerException, "İç istisna {Level}: {ErrorMessage}", level, innerException.Message);
                        innerException = innerException.InnerException;
                        level++;
                    }

                    throw; // Yeniden fırlat
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"E-posta gönderilirken hata oluştu: {to}, Konu: {subject}, Hata: {ex.Message}");
                Console.WriteLine($"E-posta gönderilirken hata oluştu: {to}, Konu: {subject}, Hata: {ex.Message}");
                _logger.LogError(ex, "E-posta gönderilirken hata oluştu: {To}, Konu: {Subject}, Hata: {ErrorMessage}",
                    to, subject, ex.Message);

                // İç içe istisnalar varsa onları da logla
                var innerException = ex.InnerException;
                int level = 1;
                while (innerException != null)
                {
                    Debug.WriteLine($"İç istisna {level}: {innerException.Message}");
                    Console.WriteLine($"İç istisna {level}: {innerException.Message}");
                    _logger.LogError(innerException, "İç istisna {Level}: {ErrorMessage}", level, innerException.Message);
                    innerException = innerException.InnerException;
                    level++;
                }

                return false;
            }
        }
    }
}
