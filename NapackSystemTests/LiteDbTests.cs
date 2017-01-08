using System;
using System.IO;
using System.Linq;
using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NapackSystemTests
{
    /// <summary>
    /// Tests Lite DB to flush out unclearly-documented functionality.
    /// </summary>
    [TestClass]
    public class LiteDbTests
    {
        private LiteDatabase database;

        [TestInitialize]
        public void TestInitialize()
        {
            string tempFile = Path.GetTempFileName();
            File.Delete(tempFile);
            database = new LiteDatabase(tempFile);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            database?.Dispose();
        }

        [TestMethod]
        public void LiteDbConcurrencyTest()
        {
            TestData testData = new TestData()
            {
                Id = "SameId",
                Data = "Unused"
            };

            TestData.ExpectedLiteDbException(() =>
            {
                LiteCollection<TestData> testDataCollection = database.GetCollection<TestData>("tests");
                testDataCollection.Insert(testData);
                testDataCollection.Insert(testData);
            }, LiteException.INDEX_DUPLICATE_KEY);
        }

        [TestMethod]
        public void LiteDbMissingItemRetrieval()
        {
            TestData testData = new TestData()
            {
                Id = "SameId",
                Data = "Unused"
            };
            
            LiteCollection<TestData> testDataCollection = database.GetCollection<TestData>("tests");
            TestData result = testDataCollection.FindById("doesNotExist");
            Assert.IsNull(result);
        }


    }

    public class TestData
    {
        public TestData() { }

        public string Id { get; set; }

        public string Data { get; set; }

        public static void ExpectedLiteDbException(Action action, int expectedCode)
        {
            try
            {
                action();
            }
            catch (LiteException le)
            {
                if (le.ErrorCode != expectedCode)
                {
                    throw new Exception("Didn't hit the expected LiteDbexception; hit " + le.ErrorCode);
                }

                return;
            }

            throw new Exception("Didn't hit the expected exception!");
        }
    }
}
