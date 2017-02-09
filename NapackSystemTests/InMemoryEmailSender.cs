using System;
using System.Collections.Generic;
using Napack.Server.Utils;

namespace NapackSystemTests
{
    public class InMemoryEmailSender : IEmailSender
    {
        public Dictionary<string, List<Tuple<string, string>>> emailsSent = new Dictionary<string, List<Tuple<string, string>>>();

        public void SendEmail(string userEmail, string subject, string body)
        {
            if (!emailsSent.ContainsKey(userEmail))
            {
                emailsSent.Add(userEmail, new List<Tuple<string, string>>());
            }

            emailsSent[userEmail].Add(Tuple.Create(subject, body));
        }
    }
}