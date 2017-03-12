using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nancy;
using Napack.Common;
using Napack.Server.Utils;
using NLog;

namespace Napack.Server
{
    /// <summary>
    /// Manages the Napack Framwork Server admin-only commands.
    /// </summary>
    public class AdminModule : NancyModule
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public const string AdminCredsFile = "../mailCreds.txt";

        public static void ValidateAdmin(NancyContext context)
        {
            Dictionary<string, string> flatHeaders = context.Request.Headers.ToDictionary(hdr => hdr.Key, hdr => string.Join(string.Empty, hdr.Value));
            if (!flatHeaders.ContainsKey(CommonHeaders.AdminId) || !flatHeaders[CommonHeaders.AdminId].Equals(AdminModule.GetAdminPassword(), StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedUserException();
            }
        }

        public static string GetAdminUserName() => DefaultAdminUserName ?? File.ReadAllLines(AdminModule.AdminCredsFile)[0];

        public static string GetAdminPassword() => DefaultAdminPassword ?? File.ReadAllLines(AdminModule.AdminCredsFile)[1];

        public AdminModule()
            : base("/admin")
        {
            // Shuts down the Napack Framework Server cleanly.
            Post["/shutdown"] = parameters =>
            {
                AdminModule.ValidateAdmin(this.Context);
                Global.ShutdownEvent.Set();
                return this.Response.AsJson(new { UtcTime = DateTime.UtcNow }, HttpStatusCode.ImATeapot);
            };
            
            // Performs the specified user modification to the given user.
            Patch["/users"] = parameters =>
            {
                UserModification userModification = SerializerExtensions.Deserialize<UserModification>(this.Context);
                AdminModule.ValidateAdmin(this.Context);

                UserIdentifier user = Global.NapackStorageManager.GetUser(userModification.UserId);
                switch (userModification.Operation)
                {
                    case Operation.DeleteUser:
                        return UsersModule.DeleteUser(this.Response, user);
                    case Operation.UpdateAccessKeys:
                        UsersModule.AssignSecretsAndSendVerificationEmail(user);
                        Global.NapackStorageManager.UpdateUser(user);
                        break;
                }

                return this.Response.AsJson(new
                {
                    OperationPerformed = userModification.Operation
                });
            };

            // Recalls a package.
            Post["/recall/{packageName}/{majorVersion}"] = parameters =>
            {
                string packageName = parameters.packageName;
                int majorVersion = int.Parse(parameters.majorVersion);
                AdminModule.ValidateAdmin(this.Context);

                NapackMetadata metadata = Global.NapackStorageManager.GetPackageMetadata(packageName, true);
                NapackMajorVersionMetadata majorVersionMetadata = metadata.GetMajorVersion(majorVersion);
                majorVersionMetadata.Recalled = true;
                Global.NapackStorageManager.UpdatePackageMetadata(metadata);

                return this.Response.AsJson(new
                {
                    VersionRecalled = majorVersion
                });
            };

            // Deletes a package.
            // 
            // Deleting a Napack involves removing:
            // - The package statistics.
            // - All of the specs.
            // - All of the packages.
            // - The metadata

            // In addition, the package is removed from.
            // - The listing of packages an author has authored.
            // - The listing of packages a user has access to.
            // - Each package that took a dependency on this package*
            //
            // Finally, an email is sent to all affected users and authorized users.
            Delete["/manage/{packageName}"] = parameters =>
            {
                string packageName = parameters.packageName;
                AdminModule.ValidateAdmin(this.Context);

                // TODO there's a lot of hardening that can be done here to prevent failures.
                NapackMetadata metadata = Global.NapackStorageManager.GetPackageMetadata(packageName, true);
                Global.NapackStorageManager.RemovePackageStatistics(packageName);

                foreach (string authorizedUser in metadata.AuthorizedUserIds)
                {
                    Global.NapackStorageManager.RemoveAuthoredPackages(authorizedUser, packageName);
                }

                Dictionary<string, List<NapackVersionIdentifier>> packagesToRemovePerAuthor = new Dictionary<string, List<NapackVersionIdentifier>>();
                foreach(KeyValuePair<int, NapackMajorVersionMetadata> majorVersion in metadata.Versions)
                {
                    foreach(KeyValuePair<int, List<int>> minorVersion in majorVersion.Value.Versions)
                    {
                        foreach (int patchVersion in minorVersion.Value)
                        {
                            NapackVersionIdentifier versionIdentifier = new NapackVersionIdentifier(packageName, majorVersion.Key, minorVersion.Key, patchVersion);
                            NapackVersion version = Global.NapackStorageManager.GetPackageVersion(versionIdentifier);
                            
                            foreach (string author in version.Authors)
                            {
                                if (!packagesToRemovePerAuthor.ContainsKey(author))
                                {
                                    packagesToRemovePerAuthor.Add(author, new List<NapackVersionIdentifier>());
                                }

                                packagesToRemovePerAuthor[author].Add(versionIdentifier);
                            }

                            Global.NapackStorageManager.RemovePackageVersion(versionIdentifier);
                            Global.NapackStorageManager.RemovePackageSpecification(versionIdentifier);
                        }
                    }
                }

                HashSet<string> affectedPackages = new HashSet<string>();
                foreach (NapackMajorVersion majorVersion in metadata.Versions.Keys.Select(value => new NapackMajorVersion(packageName, value)))
                {
                    List<NapackVersionIdentifier> consumingPackages = Global.NapackStorageManager.GetPackageConsumers(majorVersion).ToList();
                    foreach (NapackVersionIdentifier consumingPackage in consumingPackages)
                    {
                        NapackVersion version = Global.NapackStorageManager.GetPackageVersion(consumingPackage);
                        if (version.Dependencies.Remove(majorVersion))
                        {
                            Global.NapackStorageManager.UpdatePackageVersion(consumingPackage, version);
                            affectedPackages.Add(consumingPackage.NapackName);
                        }
                    }
                }

                HashSet<string> affectedUsers = new HashSet<string>();
                foreach (string affectedPackage in affectedPackages)
                {
                    NapackMetadata affectedPackageMetadata = Global.NapackStorageManager.GetPackageMetadata(affectedPackage, true);
                    foreach (string authorizedUserId in affectedPackageMetadata.AuthorizedUserIds)
                    {
                        affectedUsers.Add(authorizedUserId);
                    }
                }

                // Send the emails now that we're all done.
                foreach (string authorizedUserId in metadata.AuthorizedUserIds)
                {
                    UserIdentifier user = Global.NapackStorageManager.GetUser(authorizedUserId);
                    Global.EmailManager.SendPackageDeletionEmail(user, packageName, false);
                    Global.NapackStorageManager.UpdateUser(user);
                }

                foreach (string authorizedUserId in affectedUsers)
                {
                    UserIdentifier user = Global.NapackStorageManager.GetUser(authorizedUserId);
                    Global.EmailManager.SendPackageDeletionEmail(user, packageName, true);
                    Global.NapackStorageManager.UpdateUser(user);
                }
                
                return this.Response.AsJson(new
                {
                    AuthorizedUsersNotified = metadata.AuthorizedUserIds,
                    AffectedUsersNotified = affectedUsers,
                    Deleted = true
                }, HttpStatusCode.Gone);
            };
        }
        
        /// <summary>
        /// Provided for functional testing
        /// </summary>
        public static string DefaultAdminUserName { get; set; } = null;

        /// <summary>
        /// Provided for functional testing
        /// </summary>
        public static string DefaultAdminPassword { get; set; } = null;
    }
}