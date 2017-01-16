using System.Collections.Generic;
using System.Linq;
using Nancy;
using Napack.Server.Utils;
using NLog;

namespace Napack.Server
{
    /// <summary>
    /// Handles Napack Framwork Server User ID generation.
    /// </summary>
    public class UsersModule : NancyModule
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public UsersModule()
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
                Global.NapackStorageManager.AddUser(user);

                logger.Info($"Assigned user {user.Email} a hash and secrets, and attempted to send a validation email.");
                return this.Response.AsJson(new Common.UserSecret()
                {
                    UserId = user.Email,
                    Secrets = secret.Secrets
                });
            };

            // Confirms a user's registration.
            Patch["/"] = parameters =>
            {
                UserIdentifier user = SerializerExtensions.Deserialize<UserIdentifier>(this.Context);
                EmailManager.ValidateUserEmail(user.Email);

                UserIdentifier serverSideUser = Global.NapackStorageManager.GetUser(user.Email);
                if (!serverSideUser.EmailConfirmed && user.EmailVerificationCode == serverSideUser.EmailVerificationCode)
                {
                    serverSideUser.EmailConfirmed = true;
                }

                Global.NapackStorageManager.UpdateUser(serverSideUser);

                return this.Response.AsJson(new
                {
                    UserId = user.Email,
                    EmailValidated = serverSideUser.EmailConfirmed
                });
            };

            Delete["/"] = parameters =>
            {
                UserIdentifier user = SerializerExtensions.Deserialize<UserIdentifier>(this.Context);
                UserIdentifier.VerifyAuthorization(this.Request.Headers.ToDictionary(hdr => hdr.Key, hdr => hdr.Value), Global.NapackStorageManager, new List<string> { user.Email });

                UserIdentifier storedUser = Global.NapackStorageManager.GetUser(user.Email);

                IEnumerable<string> authorizedPackages = Global.NapackStorageManager.GetAuthorizedPackages(storedUser.Email);
                List<string> orphanedPackages = new List<string>();
                foreach (string authorizedPackage in authorizedPackages)
                {
                    NapackMetadata metadata = Global.NapackStorageManager.GetPackageMetadata(authorizedPackage);
                    metadata.AuthorizedUserIds.Remove(storedUser.Email);
                    if (metadata.AuthorizedUserIds.Any())
                    {
                        orphanedPackages.Add(authorizedPackage);
                    }

                    Global.NapackStorageManager.UpdatePackageMetadata(metadata);
                }

                Global.NapackStorageManager.RemoveUser(user);
                return this.Response.AsJson(new
                {
                    NapacksDeauthenticatedFrom = authorizedPackages,
                    OrphanedPackages = orphanedPackages,
                }, HttpStatusCode.Gone);
            };
        }
    }
}
