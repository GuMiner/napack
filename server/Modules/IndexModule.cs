using Nancy;

namespace Napack.Server
{
    /// <summary>
    /// Manages the Napack Framwork Server visible interface.
    /// </summary>
    public class IndexModule : NancyModule
    {
        public IndexModule()
        {
            Get["/"] = parameters =>
            {
                return View["Index", new IndexModel(Global.SystemConfig.AdministratorEmail)];
            };
        }
    }
}