﻿using System;
using System.ComponentModel;
using System.IO;
using System.Net.Mail;
using System.Reflection;
using Napack.Common;

namespace Napack.Server
{
    public class EmailManager
    {
        private static SmtpClient client;
        private static string emailValidationFormatString;

        public static void Initialize(string host, int port)
        {
            EmailManager.client = new SmtpClient(host, port);
            EmailManager.client.EnableSsl = true;
            EmailManager.client.SendCompleted += Client_SendCompleted;
            
            // TODO remove this duplication.
            using (StreamReader reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Napack.Server.Utils.EmailValidation_FS.txt")))
            {
                EmailManager.emailValidationFormatString = reader.ReadToEnd();
            }
        }

        private static void Client_SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Global.Log($"Email transmission status: {e.UserState as string} -> Cancelled: {e.Cancelled}. {e.Error?.ToString() ?? "No Error"}");
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
        /// Attempts to send a verification email for the specified user.
        /// </summary>
        /// <param name="user">The user to send the email for.</param>
        internal static void SendVerificationEmail(UserIdentifier user)
        {
            user.EmailVerificationCode = Guid.NewGuid();

            MailAddress from = new MailAddress(Global.AdministratorEmail, Global.AdministratorName);
            MailAddress to = new MailAddress(user.Email);

            string subject = "Napack Framework Server User Email Verification";
            user.EmailSubjectsSent.Add(subject);
            MailMessage message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = EmailManager.FormVerificationEmail(user.Email, user.EmailVerificationCode),
                IsBodyHtml = false,
            };

            try
            {
                client.SendAsync(message, user.Email);
            }
            catch (Exception ex)
            {
                EmailManager.Client_SendCompleted(null, new AsyncCompletedEventArgs(ex, false, user.Email));
            }
        }

        private static string FormVerificationEmail(string email, Guid? emailVerificationCode)
        {
            return string.Format(EmailManager.emailValidationFormatString, email, emailVerificationCode.ToString());
        }
    }
}
