using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Bootstrapper;
using Nancy.Security;
using Nancy.TinyIoc;
using System;

namespace Napack.Server
{
    class NancyBootstrapper : DefaultNancyBootstrapper
    {
        /// <summary>
        /// Modify our application startup to enable CSRF; adds an error filter.
        /// </summary>
        /// <remarks>
        /// To use this functionality, within every 'form' block, after all the inputs add '@Html.AntiForgeryToken()'.
        /// Then, within your POST reception, call 'this.ValidateCsrfToken();, catch the CsrfValidationException, and handle the result appropriately.
        /// </remarks>
        protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);
            Csrf.Enable(pipelines);

            StaticConfiguration.DisableErrorTraces = false;
            pipelines.OnError += (context, exception) =>
            {
                // TODO use NLOG
                Global.Log(exception.Message);
                return null;
            };
        }

        /// <summary>
        /// Modify our container registration so that it auto-loads our interfaces we use.
        /// </summary>
        protected override void ConfigureRequestContainer(TinyIoCContainer container, NancyContext context)
        {
            base.ConfigureRequestContainer(container, context);
            container.Register<INapackStorageManager, NapackStorageManager>();
        }


        /// <summary>
        /// Modify our root path to be the application working directory.
        /// </summary>
        protected override IRootPathProvider RootPathProvider
        {
            get { return new RootPathProvider(); }
        }
    }
}
