namespace Napack.Server.Utils
{
    /// <summary>
    /// Defines an interface to send an email.
    /// </summary>
    public interface IEmailSender
    {
        /// <summary>
        /// Sends an email to the specified address, with the given subject and body.
        /// </summary>
        void SendEmail(string userEmail, string subject, string body);
    }
}