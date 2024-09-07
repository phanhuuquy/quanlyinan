using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace QuanLyInAn.Services
{
    public class EmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _smtpUser;
        private readonly string _smtpPass;
        private readonly bool _enableSsl;

        public EmailService(IConfiguration configuration)
        {
            _smtpServer = configuration["Smtp:Host"];
            _smtpPort = int.Parse(configuration["Smtp:Port"]);
            _smtpUser = configuration["Smtp:Username"];
            _smtpPass = configuration["Smtp:Password"];
            _enableSsl = bool.Parse(configuration["Smtp:EnableSsl"]);
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_smtpUser, _smtpPass);
                smtpClient.EnableSsl = _enableSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_smtpUser),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(to);

                await smtpClient.SendMailAsync(mailMessage);
            }
        }

        public async Task SendDesignApprovalNotificationAsync(string designerEmail, string projectName, bool approved)
        {
            var subject = approved ? "Design Approved" : "Design Rejected";
            var body = $"<p>Your design for the project '{projectName}' has been {(approved ? "approved" : "rejected")}.</p>";

            await SendEmailAsync(designerEmail, subject, body);
        }
    }
}
