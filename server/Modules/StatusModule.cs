using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CSharp.RuntimeBinder;
using Nancy;
using Napack.Analyst.ApiSpec;
using Napack.Common;

namespace Napack.Server
{
    /// <summary>
    /// Handles Napack Framwork Server API graph view.
    /// </summary>
    public class StatusModule : NancyModule
    {
        public StatusModule()
            : base("/systemstatus")
        {
            // Gets the system status view.
            Get["/"] = parameters =>
            {
                return View["Status"];
            };
        }
    }
}
