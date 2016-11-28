using System;
using System.Collections.Generic;
using Nancy;
using Nancy.ModelBinding;

namespace Napack.Server
{
    /// <summary>
    /// Handles User ID generation.
    /// </summary>
    public class UserIdentifierModule : NancyModule
    {
        public UserIdentifierModule()
            : base("/userId")
        {
            // Generates a random series of user identifiers, returning them to the user.
            Post["/"] = parameters =>
            {
                string userEmail = this.Bind<string>().Substring(0, 100);
                UserIdentifier identifier = new UserIdentifier(userEmail);
                List<Guid> ids = identifier.AssignUserHash();

                Global.Log("Assigned user " + userEmail + "the following hash: " + identifier.UserHash);
                return this.Response.AsJson(new { UserEmail = userEmail, Ids = ids });
            };
        }
    }
}
