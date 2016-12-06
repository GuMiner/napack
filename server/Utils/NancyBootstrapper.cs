using System;
using System.Collections.Generic;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Responses;
using Nancy.Security;
using Nancy.TinyIoc;
using Napack.Common;

namespace Napack.Server
{
    class NancyBootstrapper : DefaultNancyBootstrapper
    {
        private IDictionary<Type, HttpStatusCode> exceptionStatusCodeMapping =
            new Dictionary<Type, HttpStatusCode>()
            {
                // 400 -- Bad Request
                [typeof(DuplicateNapackException)] = HttpStatusCode.BadRequest,
                [typeof(InvalidNapackVersionException)] = HttpStatusCode.BadRequest,

                // 401 -- Unauthorized
                [typeof(UnauthorizedUserException)] = HttpStatusCode.Unauthorized,

                // 404 -- Not Found
                [typeof(NapackNotFoundException)] = HttpStatusCode.NotFound,
                [typeof(NapackVersionNotFoundException)] = HttpStatusCode.NotFound,

                // 410 -- Gone
                [typeof(NapackRecalledException)] = HttpStatusCode.Gone
            };

        /// <summary>
        /// Modifies our application startup to enable CSRF, and reparse thrown exceptions into messages and status codes.
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
                HttpStatusCode code = HttpStatusCode.InternalServerError;
                Exception parsedException = exception as Exception;
                if (parsedException != null)
                {
                    exceptionStatusCodeMapping.TryGetValue(parsedException.GetType(), out code);
                }
                else
                {
                    parsedException = new Exception("Unable to decode the detected exception!");
                }

                JsonResponse response = new JsonResponse(new
                {
                    Type = parsedException.GetType(),
                    Message = parsedException.Message
                }, new DefaultJsonSerializer());
                
                response.StatusCode = code;
                return response;
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
