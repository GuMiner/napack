using Nancy;
using Nancy.ModelBinding;

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
                UserIdentifier user = this.Bind<UserIdentifier>();
                UserSecret secret = UserSecret.CreateNewSecret();
                user.Hash = UserIdentifier.ComputeUserHash(secret.Secrets);

                // TODO identifier validation (email). Also scan for case sensitive / insensitive errors.
                napackManager.AddUser(user);

                Global.Log("Assigned user " + user.Email + " a hash and secrets.");
                return this.Response.AsJson(new { UserId = user.Email, Secret = secret });
            };
        }
    }
}
