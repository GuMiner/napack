using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Napack.Client;

namespace NapackSystemTests
{
    /// <summary>
    /// Tests that each 
    /// </summary>
    [TestClass]
    public class NapackOperationTests
    {
        [TestMethod]
        public void NapackClientRegistersNewUser()
        {
            int returnCode = NapackClient.Main(new string[]
            {
                "Register", "test.user@invalid.com", "../../Content/NapackSettings.json"
            });

            // TODO validate output is logical.
            Assert.AreEqual(NapackClient.SUCCESS, returnCode);
        }

        [TestMethod]
        public void NapackRegistrationOperationRegistersNewUser()
        {
            RegisterOperation registerOperation = new RegisterOperation()
            {
                Operation = "Register",
                UserEmail = "test2.user@invalid.com",
                NapackSettings = "../../Content/NapackSettings.json"
            };

            registerOperation.PerformOperation();
        }
    }
}
