using Nancy;
using Napack.Server.Utils;

namespace Napack.Server
{
    /// <summary>
    /// Manages the Napack Framwork Server admin-only commands.
    /// </summary>
    public class AdminModule : NancyModule
    {
        public AdminModule() 
            : base("/admin")
        {
            // TODO all of these need authorization.
            Get["/Debug/Shutdown"] = parameters =>
            {
                Global.ShutdownEvent.Set();
                return null;
            };

            Patch["/users"] = parameters =>
            {
                UserModification userModification = SerializerExtensions.Deserialize<UserModification>(this.Context);
                return this.Response.AsJson(new
                {
                    OperationPerformed = userModification.Operation
                });
            };

            // Recalls packages.
            Post["/manage/{packageName}/{majorVersion}"] = parameters =>
            {
                string packageName = parameters.packageName;
                int majorVersion = int.Parse(parameters.majorVersion);

                return this.Response.AsJson(new
                {
                    VersionsRecalled = 0 // TODO
                });
            };

            // Deletes packages.
            Delete["/manage/{packageName}"] = parameters =>
            {
                string packageName = parameters.packageName;

                return this.Response.AsJson(new
                {
                    Deleted = true
                }, HttpStatusCode.Gone);
            };
        }
    }
}