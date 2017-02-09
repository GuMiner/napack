using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NapackSystemTests
{
    [TestClass]
    public class ServerValidationTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UserRegistrationRejectsNullEmailAsync()
        {
            string content = "{\"email\":null}";
            await SystemSetup.RestClient.PostAsync<string, string>("/users", content, new Dictionary<HttpStatusCode, System.Exception>
            {
                [HttpStatusCode.BadRequest] = new ArgumentException("Expected")
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UserRegistrationRejectsBlankEmailAsync()
        {
            string content = "{\"email\":\"  \"}";
            await SystemSetup.RestClient.PostAsync<string, string>("/users", content, new Dictionary<HttpStatusCode, System.Exception>
            {
                [HttpStatusCode.BadRequest] = new ArgumentException("Expected")
            });
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task UserRegistrationRejectsInvalidEmailAsync()
        {
            string content = "{\"email\":\"g@g@g\"}";
            await SystemSetup.RestClient.PostAsync<string, string>("/users", content, new Dictionary<HttpStatusCode, System.Exception>
            {
                [HttpStatusCode.BadRequest] = new ArgumentException("Expected")
            });
        }
    }
}
