using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        private List<string> operationLogs = new List<string>();

        [TestInitialize]
        public void TestInitialize()
        {
            NapackClientSettings settings = Serializer.Deserialize<NapackClientSettings>(File.ReadAllText(NapackOperationTests.SettingsFileLocation));

            string settingsWithDefaultUserFileLocation = NapackOperationTests.SettingsFileLocation + NapackOperationTests.ModifierSuffix;
            File.WriteAllText(settingsWithDefaultUserFileLocation, Serializer.Serialize(settings));

            NapackClient.Log = (line) => operationLogs.Add(line);
        }

        [TestMethod]
        public void NapackClientRegistersNewUser()
        {
            operationLogs.Clear();
            int returnCode = NapackClient.Main(
                $"-Operation Register -UserEmail test.user@invalid.com -NapackSettingsFile {NapackOperationTests.SettingsFileLocation} -SaveAsDefault true"
                .Split(' '));
            
            Assert.AreEqual(NapackClient.SUCCESS, returnCode);
            Assert.IsTrue(operationLogs.Any(line => line.Contains("test.user@invalid.com successfully registered")));
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

            operationLogs.Clear();
            registerOperation.PerformOperation();
            Assert.IsTrue(operationLogs.Any(line => line.Contains("test2.user@invalid.com successfully registered")));
        }

        [TestMethod]
        [ExpectedException(typeof(NapackFrameworkServerUnavailable))]
        public void NapackUploadOperationFailsWhenUserNotRegistered()
        {
            UploadOperation uploadOperation = new UploadOperation()
            {
                Operation = "Upload",
                ForceMajorUpversioning = false,
                ForceMinorUpversioning = false,
                PackageFile = NapackOperationTests.PackageJsonFileLocation,
                NapackSettingsFile = NapackOperationTests.SettingsFileLocation
            };

            uploadOperation.PerformOperation();
        }

        [TestMethod]
        public void NapackUploadOperationSuccess()
        {
            RegisterOperation registerOperation = new RegisterOperation()
            {
                Operation = "Register",
                UserEmail = "test3.user@invalid.com",
                NapackSettingsFile = NapackOperationTests.SettingsFileLocation,
                SaveAsDefault = true
            };

            registerOperation.PerformOperation();
            operationLogs.Clear();

            UploadOperation uploadOperation = new UploadOperation()
            {
                Operation = "Upload",
                ForceMajorUpversioning = false,
                ForceMinorUpversioning = false,
                PackageFile = NapackOperationTests.PackageJsonFileLocation,
                NapackSettingsFile = NapackOperationTests.SettingsFileLocation + NapackOperationTests.ModifierSuffix
            };

            uploadOperation.PerformOperation();
            Assert.IsTrue(operationLogs.Any(log => log.Contains("Created package PointInSphere")));
        }
    }
}
