using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ACGCET_Admin.Services
{
    public class EmailService
    {
        private const string SenderEmail   = "harikrishnan4665@gmail.com";
        private const string SenderPassword = "vqtt rhmb bxae cize";
        private const string SmtpHost      = "smtp.gmail.com";
        private const int    SmtpPort      = 587;

        public async Task SendOtpEmailAsync(string recipientEmail, string recipientName, string otpCode)
        {
            using var client = new SmtpClient(SmtpHost, SmtpPort)
            {
                EnableSsl   = true,
                Credentials = new NetworkCredential(SenderEmail, SenderPassword)
            };

            string body = $"""
                Dear {recipientName},

                A password reset was requested for your ACGCET Admin account.

                Your One-Time Password (OTP) is:  {otpCode}

                This OTP is valid for 10 minutes. If you did not request a password reset,
                please ignore this email or contact the system administrator immediately.

                Regards,
                ACGCET COE Office
                Alagappa Chettiar Government College of Engineering and Technology
                Karaikudi - 630 003, Tamil Nadu
                """;

            using var message = new MailMessage(SenderEmail, recipientEmail)
            {
                Subject = "ACGCET Admin — Password Reset OTP",
                Body    = body,
                IsBodyHtml = false
            };

            await client.SendMailAsync(message);
        }
    }
}
