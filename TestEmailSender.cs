using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TestEmailSender
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Gmail SMTP Test Uygulaması");
            Console.WriteLine("==========================");

            try
            {
                // SMTP ayarları
                string smtpServer = "smtp.gmail.com";
                int smtpPort = 587;
                string smtpUsername = "aliari.test@gmail.com";
                string smtpPassword = "wmdy umwt iffj oemu";
                string senderEmail = "aliari.test@gmail.com";
                string senderName = "Test Sender";
                string recipientEmail = "aliari.test@gmail.com";

                Console.WriteLine($"SMTP Ayarları: Server={smtpServer}, Port={smtpPort}, Username={smtpUsername}");

                // SMTP istemcisi oluştur
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

                // E-posta mesajı oluştur
                var message = new MailMessage
                {
                    From = new MailAddress(senderEmail, senderName),
                    Subject = "Test E-postası - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Body = $@"
                        <h2>Bu bir test e-postasıdır</h2>
                        <p>Bu e-posta, e-posta gönderim sisteminin çalışıp çalışmadığını test etmek için gönderilmiştir.</p>
                        <p>Gönderim zamanı: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}</p>
                        <p>Saygılarımızla,<br>Test Ekibi</p>
                    ",
                    IsBodyHtml = true
                };

                message.To.Add(new MailAddress(recipientEmail));

                Console.WriteLine($"E-posta gönderiliyor: From={message.From}, To={message.To}, Subject={message.Subject}");

                // E-postayı gönder
                await client.SendMailAsync(message);

                Console.WriteLine("E-posta başarıyla gönderildi!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                
                // İç içe istisnalar varsa onları da göster
                var innerException = ex.InnerException;
                int level = 1;
                while (innerException != null)
                {
                    Console.WriteLine($"İç istisna {level}: {innerException.Message}");
                    innerException = innerException.InnerException;
                    level++;
                }
            }

            Console.WriteLine("İşlem tamamlandı. Çıkmak için bir tuşa basın...");
            Console.ReadKey();
        }
    }
}
