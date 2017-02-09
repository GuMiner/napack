using System;
using System.Net.Mail;
using System.Reflection;
using Napack.Common;
using NLog;

namespace Napack.Server.Utils
{
    public class EmailManager
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IEmailSender emailSender;
        private string emailValidationFormatString;
        private string packageDeletionFormatString;

        public EmailManager(IEmailSender emailSender)
        {
            this.emailSender = emailSender;
            this.emailValidationFormatString = Serializer.ReadFromAssembly(Assembly.GetExecutingAssembly(), "Napack.Server.Utils.EmailValidation_FS.txt");
            this.packageDeletionFormatString = Serializer.ReadFromAssembly(Assembly.GetExecutingAssembly(), "Napack.Server.Utils.PackageDeletion_FS.txt");
        }

        /// <summary>
        /// Validates the user email is correct.
        /// </summary>
        /// <param name="email">The email provided.</param>
        /// <exception cref="InvalidUserIdException">If the email address is invalid.</exception>
        public void ValidateUserEmail(string email)
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
        public void SendPackageDeletionEmail(UserIdentifier user, string packageName, bool ifAffectedNotAuthorized)
        {
            string subject = "Napack Framework Server Exceptional Circumstances -- Package Deletion";
            user.EmailSubjectsSent.Add(subject);
            emailSender.SendEmail(user.Email, subject, string.Format(this.packageDeletionFormatString, user.Email, packageName, ifAffectedNotAuthorized ? 
                "are authorized to update package(s) that have may have broken dependencies after this deletion." :
                "were authorized to alter the package that has been deleted."));
        }

        /// <summary>
        /// Sends a user verification email to the specified user.
        /// </summary>
        /// <param name="user">The user to send the email for.</param>
        public void SendVerificationEmail(UserIdentifier user)
        {
            user.EmailVerificationCode = Guid.NewGuid();
            string subject = "Napack Framework Server User Email Verification";
            user.EmailSubjectsSent.Add(subject);
            emailSender.SendEmail(user.Email, subject, string.Format(this.emailValidationFormatString, user.Email, user.EmailVerificationCode));
        }
    }
}
