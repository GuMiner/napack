﻿using System.Linq;
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
            NapackFileAnalyzer.Analyze("unused", "unused", SampleStrings.MultiNamespaceClass);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNapackFileException))]
        public void NoNamespacesAreDetected()
        {
            NapackFileAnalyzer.Analyze("unused", "unused", SampleStrings.NoNamespaceClass);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidNapackFileException))]
        public void InvalidNamespacesAreDetected()
        {
            NapackFileAnalyzer.Analyze("NotTest", "unused", SampleStrings.LargeSampleClass);
        }

        [TestMethod]
        public void DocumentationIsParsed()
        {
            ClassSpec classSpec = NapackFileAnalyzer.Analyze("Test", "unused", SampleStrings.LargeSampleClass).Classes.First();
            Assert.IsNotNull(classSpec.Name);
            Assert.IsNotNull(classSpec.Name.Name);
            Assert.IsNotNull(classSpec.Name.Documentation);

            Assert.IsTrue(classSpec.PublicMethods.Count != 0);
            MethodSpec method = classSpec.PublicMethods.First();
            Assert.IsNotNull(method.Name);
            Assert.IsNotNull(method.Name.Name);
            Assert.IsNotNull(method.Name.Documentation);

            Assert.IsTrue(classSpec.PublicConstructors.Count != 0);
            ConstructorSpec constructor = classSpec.PublicConstructors.First();
            Assert.IsNotNull(constructor.Name);
            Assert.IsNotNull(constructor.Name.Name);
            Assert.IsNotNull(constructor.Name.Documentation);
        }

        [TestMethod]
        public void MethodAndClassTest()
        {
            ClassSpec classSpec = NapackFileAnalyzer.Analyze("Test", "unused", SampleStrings.LargeSampleClass).Classes.First();
            Assert.IsTrue(classSpec.PublicMethods.Count != 0);

            MethodSpec method = classSpec.PublicMethods.First();
            foreach (ParameterSpec parameter in method.Parameters)
            {
                Assert.IsNotNull(parameter.Name);
                Assert.IsNotNull(parameter.Modifier);
                Assert.IsNotNull(parameter.Type);
            }

            Assert.IsNotNull(method.ReturnType);
        }

        [TestMethod]
        public void ConstructorAndClassTest()
        {
            ClassSpec classSpec = NapackFileAnalyzer.Analyze("Test", "unused", SampleStrings.LargeSampleClass).Classes.First();
            Assert.IsTrue(classSpec.PublicConstructors.Count != 0);

            ConstructorSpec constructor = classSpec.PublicConstructors.First();

            bool oneHasDefault = false;
            foreach (ParameterSpec parameter in constructor.Parameters)
            {
                Assert.IsNotNull(parameter.Name);
                Assert.IsNotNull(parameter.Modifier);
                Assert.IsNotNull(parameter.Type);
                if (!string.IsNullOrWhiteSpace(parameter.Default))
                {
                    oneHasDefault = true;
                }
            }

            Assert.IsTrue(oneHasDefault);
        }

        [TestMethod]
        public void FieldAndClassTest()
        {
            ClassSpec classSpec = NapackFileAnalyzer.Analyze("Test", "unused", SampleStrings.LargeSampleClass).Classes.First();
            Assert.AreEqual(1, classSpec.PublicFields.Count);

            FieldSpec field = classSpec.PublicFields.First();
            Assert.IsNotNull(field.Name);
            Assert.IsNotNull(field.Name.Name);
            Assert.IsNotNull(field.Name.Documentation);
            Assert.IsNotNull(field.Type);
            Assert.IsTrue(field.IsConst);
            Assert.IsFalse(field.IsStatic);
            Assert.IsFalse(field.IsReadonly);
        }

        [TestMethod]
        public void PropertyAndClassTest()
        {
            ClassSpec classSpec = NapackFileAnalyzer.Analyze("Test", "unused", SampleStrings.LargeSampleClass).Classes.First();
            Assert.AreEqual(1, classSpec.PublicProperties.Count);

            PropertySpec property = classSpec.PublicProperties.First();
            Assert.IsNotNull(property.Name);
            Assert.IsNotNull(property.Name.Name);
            Assert.IsNotNull(property.Name.Documentation);
            Assert.IsNotNull(property.Type);
            Assert.IsFalse(property.IsStatic);
        }
    }
}
