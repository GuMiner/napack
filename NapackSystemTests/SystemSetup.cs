using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Napack.Server;

namespace NapackSystemTests
{
    [TestClass]
    public class SystemSetup
    {
        private static Task napackServerTask;

        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext testContent)
        {
            napackServerTask = Task.Factory.StartNew(() =>
            {
                Global.Main(new[] { "http://localhost:9876" });
            }, TaskCreationOptions.LongRunning);
            
            while (!Global.Initialized)
            {
                Thread.Sleep(100);
            }
        }

        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
            Global.ShutdownEvent.Set();
            while (!napackServerTask.IsCompleted)
            {
                Thread.Sleep(100);
            }
        }
    }
}
