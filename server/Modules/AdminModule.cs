using Nancy;

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
        }
    }
}