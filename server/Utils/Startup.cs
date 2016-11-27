using Owin;

namespace Napack.Server
{
    /// <summary>
    /// Indicates to OWIN that we're using Nancy.
    /// </summary>
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy(options => options.Bootstrapper = new NancyBootstrapper());
        }
    }
}
