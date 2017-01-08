using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Napack.Client;
using Napack.Common;

namespace NapackSystemTests
{
    /// <summary>
    /// Tests that each major napack operation works.
    /// </summary>
    [TestClass]
    public class NapackOperationTests
    {
        private const string SettingsFileLocation = "../../Content/NapackSettings.json";
        private const string PackageJsonFileLocation = "../../Content/Napack/PointInSphere.json";
        private const string ModifierSuffix = ".mod";

        [TestInitialize]
        public void TestInitialize()
        {
            NapackClientSettings settings = Serializer.Deserialize<NapackClientSettings>(File.ReadAllText(NapackOperationTests.SettingsFileLocation));

            // TODO fix.
            // settings.DefaultUserId = SystemSetup.AuthorizedUser.UserId;
            // settings.DefaultUserAuthenticationKeys = SystemSetup.AuthorizedUser.Secrets;

            string settingsWithDefaultUserFileLocation = NapackOperationTests.SettingsFileLocation + NapackOperationTests.ModifierSuffix;
            File.WriteAllText(settingsWithDefaultUserFileLocation, Serializer.Serialize(settings));
        }

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
                NapackSettingsFile = NapackOperationTests.SettingsFileLocation
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
                PackageFile = NapackOperationTests.PackageJsonFileLocation,
                NapackSettingsFile = NapackOperationTests.SettingsFileLocation
            };

            uploadOperation.PerformOperation();
        }

        [TestMethod]
        public void NapackUploadOperationSuccess()
        {
            UploadOperation uploadOperation = new UploadOperation()
            {
                Operation = "Upload",
                ForceMajorUpversioning = false,
                ForceMinorUpversioning = false,
                UpdateMetadata = false,
                PackageFile = NapackOperationTests.PackageJsonFileLocation,
                NapackSettingsFile = NapackOperationTests.SettingsFileLocation + NapackOperationTests.ModifierSuffix
            };

            uploadOperation.PerformOperation();
        }
    }
}
