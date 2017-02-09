using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Napack.Server;

namespace NapackSystemTests
{
    [TestClass]
    public class DefinitionTests
    {
        private Mock<INapackStorageManager> napackStorageManager;
        private UserSecret testUserSecret;
        private UserIdentifier testUser;

        [TestInitialize]
        public void TestInitialize()
        {
            napackStorageManager = new Mock<INapackStorageManager>(MockBehavior.Strict);
            napackStorageManager.Setup(sm => sm.GetUser(It.IsAny<string>())).Returns(() => testUser);

            testUserSecret = UserSecret.CreateNewSecret();
        }

        [TestMethod]
        public void UserSecretPopulatesSecrets()
        {
            UserSecret secret = UserSecret.CreateNewSecret();
            Assert.IsNotNull(secret);
            Assert.AreEqual(UserSecret.SecretCount, secret.Secrets.Count);
            
            foreach (Guid secretPart in secret.Secrets)
            {
                Assert.AreEqual(1, secret.Secrets.Count(part => part.Equals(secretPart)));
            }
        }

        [TestMethod]
        public void UserIdentifierCanComputeHash()
        {
            UserSecret secret = UserSecret.CreateNewSecret();
            string hash = UserIdentifier.ComputeUserHash(secret.Secrets);
            Assert.IsNotNull(hash);
        }

        [TestMethod]
        [ExpectedException(typeof(Napack.Common.UnauthorizedUserException))]
        public void UserIdentifierVerifyAuthVerifiesHeaders()
        {
            UserIdentifier.VerifyAuthorization(
                new Dictionary<string, IEnumerable<string>>(),
                this.napackStorageManager.Object,
                new List<string> { "fake" });
        }

        [TestMethod]
        [ExpectedException(typeof(Napack.Common.UnauthorizedUserException))]
        public void UserIdentifierVerifyAuthDecodesHeaders()
        {
            UserIdentifier.VerifyAuthorization(
                new Dictionary<string, IEnumerable<string>>()
                {
                    ["UserKeys"] = new[] { "invalid" },
                    ["UserId"] = new[] { "invalid" }

                },
                this.napackStorageManager.Object,
                new List<string> { "fake" });
        }

        [TestMethod]
        [ExpectedException(typeof(Napack.Common.UnauthorizedUserException))]
        public void UserIdentiferVerifyAuthVerifiesUserIsAuthorized()
        {
            UserIdentifier.VerifyAuthorization(
                new Dictionary<string, IEnumerable<string>>()
                {
                    ["UserKeys"] = new[] { Convert.ToBase64String(Encoding.UTF8.GetBytes(Napack.Common.Serializer.Serialize(testUserSecret.Secrets))) },
                    ["UserId"] = new[] { "fake" }

                },
                this.napackStorageManager.Object,
                new List<string> { "notHere" });
        }

        [TestMethod]
        [ExpectedException(typeof(Napack.Common.UnauthorizedUserException))]
        public void UserIdentiferVerifyAuthRequiresEmailValidation()
        {
            bool requiredSetting = Global.SystemConfig.RequireEmailValidation;
            Global.SystemConfig.RequireEmailValidation = true;
            try
            {
                testUser = this.FormTestUser(false);

                UserIdentifier.VerifyAuthorization(
                    new Dictionary<string, IEnumerable<string>>()
                    {
                        ["UserKeys"] = new[] { Convert.ToBase64String(Encoding.UTF8.GetBytes(Napack.Common.Serializer.Serialize(testUserSecret.Secrets))) },
                        ["UserId"] = new[] { "fake" }

                    },
                    this.napackStorageManager.Object,
                    new List<string> { "fake" });
            }
            finally
            {
                Global.SystemConfig.RequireEmailValidation = false;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Napack.Common.UnauthorizedUserException))]
        public void UserIdentiferVerifyAuthCanSkipEmailValidation()
        {
            bool requiredSetting = Global.SystemConfig.RequireEmailValidation;
            Global.SystemConfig.RequireEmailValidation = false;
            try
            {
                testUser = this.FormTestUser(false);

                UserIdentifier.VerifyAuthorization(
                    new Dictionary<string, IEnumerable<string>>()
                    {
                        ["UserKeys"] = new[] { Convert.ToBase64String(Encoding.UTF8.GetBytes(Napack.Common.Serializer.Serialize(UserSecret.CreateNewSecret()))) },
                        ["UserId"] = new[] { "fake" }

                    },
                    this.napackStorageManager.Object,
                    new List<string> { "fake" });
            }
            finally
            {
                Global.SystemConfig.RequireEmailValidation = requiredSetting;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(Napack.Common.UnauthorizedUserException))]
        public void UserIdentiferVerifyAuthValidatesUserHash()
        {
            testUser = this.FormTestUser(true);

            UserIdentifier.VerifyAuthorization(
                new Dictionary<string, IEnumerable<string>>()
                {
                    ["UserKeys"] = new[] { Convert.ToBase64String(Encoding.UTF8.GetBytes(Napack.Common.Serializer.Serialize(UserSecret.CreateNewSecret()))) },
                    ["UserId"] = new[] { "fake" }

                },
                this.napackStorageManager.Object,
                new List<string> { "fake" });
        }

        [TestMethod]
        public void UserIdentiferVerifyAuthSuccess()
        {
            testUser = this.FormTestUser(true);

            UserIdentifier.VerifyAuthorization(
                new Dictionary<string, IEnumerable<string>>()
                {
                    ["UserKeys"] = new[] { Convert.ToBase64String(Encoding.UTF8.GetBytes(Napack.Common.Serializer.Serialize(testUserSecret.Secrets))) },
                    ["UserId"] = new[] { "fake" }

                },
                this.napackStorageManager.Object,
                new List<string> { "fake" });
        }

        private UserIdentifier FormTestUser(bool emailConfirmed = true)
        {
            return new UserIdentifier()
            {
                Email = "fake",
                EmailConfirmed = emailConfirmed,
                Hash = UserIdentifier.ComputeUserHash(testUserSecret.Secrets)
            };
        }
    }
}
