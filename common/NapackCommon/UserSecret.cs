using System;
using System.Collections.Generic;

namespace Napack.Common
{
    /// <summary>
    /// Defines the contract for a registered user secret created server-side and sent client-side.
    /// </summary>
    public class UserSecret
    {
        public string UserId { get; set; }

        public List<Guid> Secrets { get; set; }
    }
}
