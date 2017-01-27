using System;
using System.ComponentModel;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using Napack.Common;
using NLog;

namespace Napack.Server
{
    public class EmailManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        
        private static SmtpClient client;
        private static string emailValidationFormatString;
        private static string packageDeletionFormatString;

        public static void Initialize(string host, int port)
        {
            EmailManager.client = new SmtpClient(host, port);
            EmailManager.client.EnableSsl = true;
            EmailManager.client.Credentials = new NetworkCredential(AdminModule.GetAdminUserName(), AdminModule.GetAdminPassword());
            EmailManager.client.SendCompleted += Client_SendCompleted;
            
            EmailManager.emailValidationFormatString = Serializer.ReadFromAssembly(Assembly.GetExecutingAssembly(), "Napack.Server.Utils.EmailValidation_FS.txt");
            EmailManager.packageDeletionFormatString = Serializer.ReadFromAssembly(Assembly.GetExecutingAssembly(), "Napack.Server.Utils.PackageDeletion_FS.txt");
            
        }

        private static void Client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            logger.Info($"Email transmission status: {e.UserState as string} -> Cancelled: {e.Cancelled}. {e.Error?.ToString() ?? "No Error"}");
        }

        /// <summary>
        /// Validates the user email is correct.
        /// </summary>
        /// <param name="email">The email provided.</param>
        /// <exception cref="InvalidUserIdException">If the email address is invalid.</exception>
        public static void ValidateUserEmail(string email)
        {
            try
            {
                MailAddress address = new MailAddress(email);
                if (address.Address != email)
                {
                    // We have to check the address is the same because the built-in parser may return a different address based on the parsing rules.
                    throw new Exception();
                }
            }
            catch
            {
                throw new InvalidUserIdException();
            }
        }

        /// <summary>
        /// Sends a package deleted email to the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user authorized/affected.</param>
        /// <param name="packageName">The name of the package being deleted.</param>
        /// <param name="ifAffectedNotAuthorized">If true, the user was affected indirectly. If false, the user was authorized to ask for the deletion.</param>
        internal static void SendPackageDeletionEmail(UserIdentifier user, string packageName, bool ifAffectedNotAuthorized)
        {
            string subject = "Napack Framework Server Exceptional Circumstances -- Package Deletion";
            user.EmailSubjectsSent.Add(subject);
            SendGenericEmail(user.Email, subject, string.Format(EmailManager.packageDeletionFormatString, user.Email, packageName, ifAffectedNotAuthorized ? 
                "are authorized to update package(s) that have may have broken dependencies after this deletion." :
                "were authorized to alter the package that has been deleted."));
        }

        /// <summary>
        /// Sends a user verification email to the specified user.
        /// </summary>
        /// <param name="user">The user to send the email for.</param>
        internal static void SendVerificationEmail(UserIdentifier user)
        {
            user.EmailVerificationCode = Guid.NewGuid();
            string subject = "Napack Framework Server User Email Verification";
            user.EmailSubjectsSent.Add(subject);
            SendGenericEmail(user.Email, subject, string.Format(EmailManager.emailValidationFormatString, user.Email, user.EmailVerificationCode));
        }

        private static void SendGenericEmail(string userEmail, string subject, string body)
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
                EmailManager.Client_SendCompleted(null, new AsyncCompletedEventArgs(ex, false, userEmail));
            }
        }
    }
}
