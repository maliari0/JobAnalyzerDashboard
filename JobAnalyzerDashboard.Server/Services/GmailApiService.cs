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

                var token = await _oauthService.GetOAuthTokenByProfileIdAndProvider(profileId, "Google");
                if (token == null)
                {
                    _logger.LogWarning("OAuth token bulunamadı: ProfileId={ProfileId}, Provider=Google", profileId);
                    throw new Exception($"OAuth token bulunamadı: ProfileId={profileId}, Provider=Google");
                }

                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("JobAnalyzerDashboard", token.Email));
                message.To.Add(new MailboxAddress("", to));
                message.Subject = subject;

                string htmlBody = body.Replace("\n", "<br>").Replace("\r\n", "<br>");

                if (!htmlBody.Contains("<html>") && !htmlBody.Contains("<body>"))
                {
                    htmlBody = $@"
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; line-height: 1.6; }}
                            p {{ margin-bottom: 10px; }}
                        </style>
                    </head>
                    <body>
                        {htmlBody}
                    </body>
                    </html>";
                }

                var builder = new BodyBuilder();
                builder.HtmlBody = htmlBody;

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
            catch (Google.GoogleApiException gex) when (gex.Error?.Code == 403)
            {
                _logger.LogError(gex, "Gmail API is not enabled or authorized: {To}, Subject: {Subject}, Error: {Error}", to, subject, gex.Message);
                throw new Exception("Gmail API is not enabled or authorized. Please enable Gmail API in Google Cloud Console and try again.", gex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email via Gmail: {To}, Subject: {Subject}", to, subject);
                return false;
            }
        }
    }
}
