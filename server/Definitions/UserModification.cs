using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Napack.Common;
using Newtonsoft.Json;

namespace Napack.Server
{
    public enum Operation
    {
        UpdateAccessKeys,
        DeleteUser
    };

    public class UserModification
    {
        string UserId { get; set; }

        public Operation Operation { get; set; }
    }
}
