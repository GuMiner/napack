using System;
using System.Collections.Generic;
using System.IO;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.IO;
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
                [typeof(ExcessiveNapackException)] = HttpStatusCode.BadRequest,
                [typeof(InvalidNapackNameException)] = HttpStatusCode.BadRequest,
                [typeof(InvalidNapackFileException)] = HttpStatusCode.BadRequest,
                [typeof(InvalidNapackFileExtensionException)] = HttpStatusCode.BadRequest,
                [typeof(InvalidNamespaceException)] = HttpStatusCode.BadRequest,
                [typeof(UnsupportedNapackFileException)] = HttpStatusCode.BadRequest,

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

            pipelines.BeforeRequest += (ctx) =>
            {
                NancyContext context = ctx as NancyContext;

                // Technically this is 7.4% over 1 MiB. I'm not going to be pedantic.
                int maxIterations = 12;
                int hundredKiB = 100 * 1024;
                byte[] hundredK = new byte[hundredKiB];
                RequestStream requestStream = context.Request.Body;
                for (int i = 0; i < maxIterations; i++)
                {
                    // This unfortunately means we're processing the request stream twice.
                    if (requestStream.Read(hundredK, 0, hundredKiB) != hundredKiB)
                    {
                        // The request is under 1 MiB, so continue processing. Reset the stream so deserializing the JSON works.
                        requestStream.Seek(0, SeekOrigin.Begin);
                        return null;
                    }
                }

                return this.GenerateJsonException(new ExcessiveNapackException(), HttpStatusCode.BadRequest);
            };

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

                return this.GenerateJsonException(parsedException, code);
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

        private JsonResponse GenerateJsonException(Exception exception, HttpStatusCode code)
        {
            JsonResponse response = new JsonResponse(new
            {
                Type = exception.GetType(),
                Message = exception.Message
            }, new DefaultJsonSerializer());

            response.StatusCode = code;
            return response;
        }
    }
}
