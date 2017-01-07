using System;
using System.Collections.Generic;

namespace Napack.Client
{
    /// <summary>
    /// Stores default credentials for later reuse.
    /// </summary>
    internal class DefaultCredentials
    {
        public DefaultCredentials()
        {
        }

        public DefaultCredentials(string userId, List<Guid> secrets)
        {
            this.UserId = userId;
            this.Secrets = secrets;
        }

        public string UserId { get; set; }

        public List<Guid> Secrets { get; set; }
    }
}