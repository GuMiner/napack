using System;
using System.Collections.Generic;

namespace Napack.Server
{
    public class UserSecret
    {
        public UserSecret()
        {
            this.Secrets = new List<Guid>();
        }

        /// <summary>
        /// The GUIDs that compose the user's secret.
        /// </summary>
        public List<Guid> Secrets { get; set; }

        public static UserSecret CreateNewSecret()
        {
            const int secretCount = 3;

            UserSecret secret = new UserSecret();
            for (int i = 0; i < secretCount; i++)
            {
                secret.Secrets.Add(Guid.NewGuid());
            }

            return secret;
        }
    }
}
