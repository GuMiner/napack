using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using NLog;

namespace Napack.Server.Utils
{
    public class SmtpEmailSender : IEmailSender
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private SmtpClient client;

        public SmtpEmailSender(string emailHost, int emailPort)
        {
            this.client = new SmtpClient(emailHost, emailPort);
            this.client.EnableSsl = true;
            this.client.Credentials = new NetworkCredential(AdminModule.GetAdminUserName(), AdminModule.GetAdminPassword());
            this.client.SendCompleted += SmtpEmailSender.Client_SendCompleted;
        }

        private static void Client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            logger.Info($"Email transmission status: {e.UserState as string} -> Cancelled: {e.Cancelled}. {e.Error?.ToString() ?? "No Error"}");
        }

        public void SendEmail(string userEmail, string subject, string body)
        {
            MailAddress from = new MailAddress(Global.SystemConfig.AdministratorEmail, Global.SystemConfig.AdministratorName);
            MailAddress to = new MailAddress(userEmail);

            MailMessage message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = false,
            };

            try
            {
                logger.Info($"Sending email to user {userEmail} about {subject}.");
                client.SendAsync(message, userEmail);
            }
            catch (Exception ex)
            {
                SmtpEmailSender.Client_SendCompleted(null, new AsyncCompletedEventArgs(ex, false, userEmail));
            }
        }
    }
}