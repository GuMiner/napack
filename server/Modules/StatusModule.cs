using Nancy;

namespace Napack.Server
{
    /// <summary>
    /// Handles Napack Framwork Server status display
    /// </summary>
    public class StatusModule : NancyModule
    {
        public StatusModule()
            : base("/systemstatus")
        {
            // Gets the system status view.
            Get["/"] = parameters =>
            {
                return View["Status", new StatusModel(Global.SystemStats.TotalCallsSinceUptime, Global.SystemStats.UniqueIpsSinceUptime, Global.SystemStats.Uptime)];
            };
        }
    }
}
