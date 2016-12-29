using Nancy;
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

                EmailManager.ValidateUserEmail(user.Email);
                UserSecret secret = UserSecret.CreateNewSecret();
                user.Reset(UserIdentifier.ComputeUserHash(secret.Secrets));

                EmailManager.SendVerificationEmail(user);
                napackManager.AddUser(user);

                Global.Log("Assigned user " + user.Email + " a hash and secrets, and attempted to send a validation email.");
                return this.Response.AsJson(new Common.UserSecret()
                {
                    UserId = user.Email,
                    Secrets = secret.Secrets
                });
            };
        }
    }
}
