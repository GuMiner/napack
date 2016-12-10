using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;
using Napack.Common;

namespace NapackAnalystTests
{
    [TestClass]
    public class NapackClassAnalyzerTests
    {
        [TestMethod]
        [ExpectedException(typeof(UnsupportedNapackFileException))]
        public void MultipleNamespacesAreUnsupported()
        {
            NapackClassAnalyzer.Analyze("unused", "unused", SampleStrings.MultiNamespaceClass);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNapackFileException))]
        public void NoNamespacesAreDetected()
        {
            NapackClassAnalyzer.Analyze("unused", "unused", SampleStrings.NoNamespaceClass);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNapackFileException))]
        public void InvalidNamespacesAreDetected()
        {
            NapackClassAnalyzer.Analyze("NotTest", "unused", SampleStrings.DocumentationTest);
        }

        [TestMethod]
        public void DocumentationIsParsed()
        {
            ClassSpec classSpec = NapackClassAnalyzer.Analyze("Test", "unused", SampleStrings.DocumentationTest).First();
            Assert.IsNotNull(classSpec.Name);
            Assert.IsNotNull(classSpec.Name.Name);
            Assert.IsNotNull(classSpec.Name.Documentation);
        }
    }
}
