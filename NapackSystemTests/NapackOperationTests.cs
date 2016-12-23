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
        private const string SettingsFileLocation = "../../Content/NapackSettings.json";
        private const string PackageJsonFileLocation = "../../Content/Napack/PointInSphere.json";

        [TestMethod]
        public void NapackClientRegistersNewUser()
        {
            int returnCode = NapackClient.Main(new string[]
            {
                "Register", "test.user@invalid.com", NapackOperationTests.SettingsFileLocation
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
                NapackSettings = NapackOperationTests.SettingsFileLocation
            };

            registerOperation.PerformOperation();
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void NapackUploadOperationFailsWhenNoAuthorizedUser()
        {
            UploadOperation uploadOperation = new UploadOperation()
            {
                Operation = "Upload",
                ForceMajorUpversioning = false,
                ForceMinorUpversioning = false,
                UpdateMetadata = false,
                PackageJsonFile = NapackOperationTests.PackageJsonFileLocation,
                NapackSettings = NapackOperationTests.SettingsFileLocation
            };

            uploadOperation.PerformOperation();
        }
    }
}
