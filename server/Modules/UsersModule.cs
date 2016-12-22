﻿using Nancy;
using Napack.Server.Utils;

namespace Napack.Server
{
    /// <summary>
    /// Handles Napack Framwork Server User ID generation.
    /// </summary>
    public class UsersModule : NancyModule
    {
        public UsersModule(INapackStorageManager napackManager)
            : base("/users")
        {
            // Generates a random series of user identifiers, returning them to the user.
            Post["/"] = parameters =>
            {
                UserIdentifier user = SerializerExtensions.Deserialize<UserIdentifier>(this.Context);
                UserSecret secret = UserSecret.CreateNewSecret();
                user.Hash = UserIdentifier.ComputeUserHash(secret.Secrets);

                // TODO identifier validation (email). Also scan for case sensitive / insensitive errors.
                napackManager.AddUser(user);

                Global.Log("Assigned user " + user.Email + " a hash and secrets.");
                return this.Response.AsJson(new Common.UserSecret()
                {
                    UserId = user.Email,
                    Secrets = secret.Secrets
                });
            };
        }
    }
}
