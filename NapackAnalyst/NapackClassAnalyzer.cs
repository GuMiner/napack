using System;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Napack.Analyst.ApiSpec;
using Napack.Common;

namespace Napack.Analyst
{
    /// <summary>
    /// Defines how to perform class analysis.
    /// </summary>
    internal class NapackClassAnalyzer
    {
        private static CSharpParseOptions ParseOptions = 
            new CSharpParseOptions(LanguageVersion.Default, DocumentationMode.Diagnose, SourceCodeKind.Regular);
        private static TimeSpan MaxParseTime = TimeSpan.FromMinutes(1);

        /// <summary>
        /// Analyzes a Napack class file to determine the class specification it contains.
        /// </summary>
        /// <exception cref="InvalidNamespaceException">If a compilable Napack file is in the wrong namespace.</exception>
        /// <exception cref="InvalidNapackFileException">If a Napack file is listed with MSBuild type <see cref="NapackFile.ContentType"/>, but could not be analyzed.</exception>
        /// <exception cref="UnsupportedNapackFileException">If a Napack file uses C# functionality or syntax that the Napack system explicitly prohibits.</exception>
        public static NapackClassSpec Analyze(string filename, string contents)
        {
            // Ensure we don't hang the server with someone sending invalid class files that take forever to parse.
            CancellationTokenSource cts = new CancellationTokenSource(NapackClassAnalyzer.MaxParseTime);

            try
            {
                SyntaxTree tree = CSharpSyntaxTree.ParseText(contents, NapackClassAnalyzer.ParseOptions, string.Empty, Encoding.Default, cts.Token);
                return AnalyzeSyntaxTree(tree);
            }
            catch (Exception ex)
            {
                throw new InvalidNapackFileException(ex.GetType().ToString() + ": " + ex.Message);
            }
        }

        private static NapackClassSpec AnalyzeSyntaxTree(SyntaxTree tree)
        {
            throw new NotImplementedException();
        }
    }
}