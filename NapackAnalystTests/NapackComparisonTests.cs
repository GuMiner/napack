using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;

namespace NapackAnalystTests
{
    [TestClass]
    public class NapackComparisonTests
    {
        [TestMethod]
        public void IdenticalApisCausePatch()
        {
            ClassSpec classSpec = NapackClassAnalyzer.Analyze("Test", "unused", SampleStrings.LargeSampleClass).First();
            NapackSpec spec = new NapackSpec();
            spec.Classes.Add(classSpec);

            Assert.AreEqual(NapackAnalyst.UpversionType.Patch, NapackAnalyst.DeterminedRequiredUpversioning(spec, spec));
        }
    }
}
