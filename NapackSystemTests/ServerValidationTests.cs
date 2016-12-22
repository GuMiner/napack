using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Napack.Server;

namespace NapackSystemTests
{
    [TestClass]
    public class ServerValidationTests
    {
        [TestMethod]
        public void UserRegistrationRejectsNullEmail()
        {
            string content = "{\"email\":null}";
        }
    }
}
