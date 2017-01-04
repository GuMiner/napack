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

                UserIdentifier serverSideUser = napackManager.GetUser(user.Email);
                if (!serverSideUser.EmailConfirmed && user.EmailVerificationCode == serverSideUser.EmailVerificationCode)
                {
                    serverSideUser.EmailConfirmed = true;
                }

                napackManager.UpdateUser(serverSideUser);

                return this.Response.AsJson(new
                {
                    UserId = user.Email,
                    EmailValidated = user.EmailConfirmed
                });
            };

            Delete["/"] = parameters =>
            {
                UserIdentifier user = SerializerExtensions.Deserialize<UserIdentifier>(this.Context);
                UserIdentifier.VerifyAuthorization(this.Request.Headers.ToDictionary(hdr => hdr.Key, hdr => hdr.Value), napackManager, new List<string> { user.Email });

                UserIdentifier storedUser = napackManager.GetUser(user.Email);

                IEnumerable<string> authorizedPackages = napackManager.GetAuthorizedPackages(storedUser.Email);
                List<string> orphanedPackages = new List<string>();
                foreach (string authorizedPackage in authorizedPackages)
                {
                    NapackMetadata metadata = napackManager.GetPackageMetadata(authorizedPackage);
                    metadata.AuthorizedUserIds.Remove(storedUser.Email);
                    if (metadata.AuthorizedUserIds.Any())
                    {
                        orphanedPackages.Add(authorizedPackage);
                    }

                    napackManager.UpdatePackageMetadata(metadata);
                }

                napackManager.RemoveUser(user);
                return this.Response.AsJson(new
                {
                    NapacksDeauthenticatedFrom = authorizedPackages,
                    OrphanedPackages = orphanedPackages,
                }, HttpStatusCode.Gone);
            };
        }
    }
}
