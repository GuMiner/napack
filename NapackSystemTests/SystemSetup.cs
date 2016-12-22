using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Napack.Common;
using Napack.Server;

namespace NapackSystemTests
{
    [TestClass]
    public class SystemSetup
    {
        private static Task napackServerTask;

        public static RestClient RestClient;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContent)
        {
            string localServer = "http://localhost:9876";

            napackServerTask = Task.Factory.StartNew(() =>
            {
                Global.Main(new[] { localServer });
            }, TaskCreationOptions.LongRunning);

            SystemSetup.RestClient = new RestClient(new Uri(localServer));

            while (!Global.Initialized)
            {
                Thread.Sleep(100);
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            SystemSetup.RestClient?.Dispose();
            SystemSetup.RestClient = null;

            Global.ShutdownEvent.Set();
            while (!napackServerTask.IsCompleted)
            {
                Thread.Sleep(100);
            }
        }
    }
}
