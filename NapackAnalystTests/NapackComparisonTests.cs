using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Napack.Analyst;
using Napack.Analyst.ApiSpec;

namespace NapackAnalystTests
{
    [TestClass]
    public class NapackComparisonTests
    {
        private NapackSpec oldSpec;

        [TestInitialize]
        public void TestInitialize()
        {
            oldSpec = this.LoadNapackSpec(SampleStrings.LargeSampleClass);
        }

        [TestMethod]
        public void IdenticalApisCausePatch()
        {
            Assert.AreEqual(NapackAnalyst.UpversionType.Patch, NapackAnalyst.DeterminedRequiredUpversioning(oldSpec, oldSpec));
        }

        [TestMethod]
        public void MinorPropertyChangeCausesMinor()
        {
            NapackSpec minorPropertySpec = this.LoadNapackSpec(SampleStrings.LargeSampleMinorPropertyChange);
            Assert.AreEqual(NapackAnalyst.UpversionType.Minor, NapackAnalyst.DeterminedRequiredUpversioning(oldSpec, minorPropertySpec));
        }

        [TestMethod]
        public void MinorFieldChangeCausesMinor()
        {
            NapackSpec minorFieldSpec = this.LoadNapackSpec(SampleStrings.LargeSampleMinorFieldChange);
            Assert.AreEqual(NapackAnalyst.UpversionType.Minor, NapackAnalyst.DeterminedRequiredUpversioning(oldSpec, minorFieldSpec));
        }

        [TestMethod]
        public void MinorConstructorChangeCausesMinor()
        {
            NapackSpec minorConstructorSpec = this.LoadNapackSpec(SampleStrings.LargeSampleMinorConstructorChange);
            Assert.AreEqual(NapackAnalyst.UpversionType.Minor, NapackAnalyst.DeterminedRequiredUpversioning(oldSpec, minorConstructorSpec));
        }

        [TestMethod]
        public void MinorMethodChangeCausesMinor()
        {
            NapackSpec minorMethodSpec = this.LoadNapackSpec(SampleStrings.LargeSampleMinorMethodChange);
            Assert.AreEqual(NapackAnalyst.UpversionType.Minor, NapackAnalyst.DeterminedRequiredUpversioning(oldSpec, minorMethodSpec));
        }

        [TestMethod]
        public void MajorPropertyChangeCausesMajor()
        {
            NapackSpec minorPropertySpec = this.LoadNapackSpec(SampleStrings.LargeSampleMajorPropertyChange);
            Assert.AreEqual(NapackAnalyst.UpversionType.Major, NapackAnalyst.DeterminedRequiredUpversioning(oldSpec, minorPropertySpec));
        }

        [TestMethod]
        public void MajorFieldChangeCausesMajor()
        {
            NapackSpec minorFieldSpec = this.LoadNapackSpec(SampleStrings.LargeSampleMajorFieldChange);
            Assert.AreEqual(NapackAnalyst.UpversionType.Major, NapackAnalyst.DeterminedRequiredUpversioning(oldSpec, minorFieldSpec));
        }

        [TestMethod]
        public void MajorConstructorChangeCausesMajor()
        {
            NapackSpec minorConstructorSpec = this.LoadNapackSpec(SampleStrings.LargeSampleMajorConstructorChange);
            Assert.AreEqual(NapackAnalyst.UpversionType.Major, NapackAnalyst.DeterminedRequiredUpversioning(oldSpec, minorConstructorSpec));
        }

        [TestMethod]
        public void MajorMethodChangeCausesMajor()
        {
            NapackSpec minorMethodSpec = this.LoadNapackSpec(SampleStrings.LargeSampleMajorMethodChange);
            Assert.AreEqual(NapackAnalyst.UpversionType.Major, NapackAnalyst.DeterminedRequiredUpversioning(oldSpec, minorMethodSpec));
        }

        private NapackSpec LoadNapackSpec(string content)
        {
            ClassSpec classSpec = NapackClassAnalyzer.Analyze("Test", "unused", content).First();
            NapackSpec spec = new NapackSpec();
            spec.Classes.Add(classSpec);

            return spec;
        }
    }
}
