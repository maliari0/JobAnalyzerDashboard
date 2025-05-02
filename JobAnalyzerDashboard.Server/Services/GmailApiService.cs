using System;
using System.IO;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace JobAnalyzerDashboard.Server.Services
{
    public class GmailApiService
    {
        private readonly OAuthService _oauthService;
        private readonly ILogger<GmailApiService> _logger;

        public GmailApiService(OAuthService oauthService, ILogger<GmailApiService> logger)
        {
            _oauthService = oauthService;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(int profileId, string to, string subject, string body, string attachmentPath = null)
        {
            try
            {
                var accessToken = await _oauthService.GetValidAccessTokenAsync(profileId, "Google");

                var service = new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = GoogleCredential.FromAccessToken(accessToken),
                    ApplicationName = "JobAnalyzerDashboard"
                });

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("JobAnalyzerDashboard", "me"));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                var builder = new BodyBuilder();
                builder.HtmlBody = body;

                if (!string.IsNullOrEmpty(attachmentPath) && File.Exists(attachmentPath))
                {
                    builder.Attachments.Add(attachmentPath);
                }

                message.Body = builder.ToMessageBody();

                using (var ms = new MemoryStream())
                {
                    message.WriteTo(ms);
                    ms.Position = 0;

                    var gmailMessage = new Message
                    {
                        Raw = Convert.ToBase64String(ms.ToArray())
                            .Replace('+', '-')
                            .Replace('/', '_')
                            .Replace("=", "")
                    };

                    var result = await service.Users.Messages.Send(gmailMessage, "me").ExecuteAsync();

                    _logger.LogInformation("Email sent via Gmail: {To}, Subject: {Subject}, MessageId: {MessageId}",
                        to, subject, result.Id);

                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email via Gmail: {To}, Subject: {Subject}", to, subject);
                return false;
            }
        }
    }
}
