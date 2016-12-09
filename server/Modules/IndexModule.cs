﻿using Nancy;

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
                Global.Log("Index");
                return View["Index"];
            };

            // TODO remove this once I've determined a better way of functional testing Nancy routing.
            Get["/Debug/Shutdown"] = parameters =>
            {
                Global.Log("Shutdown");
                Global.ShutdownEvent.Set();
                return null;
            };
        }
    }
}