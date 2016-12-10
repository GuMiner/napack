using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
        public static ICollection<ClassSpec> Analyze(string napackName, string filename, string contents)
        {
            // Ensure we don't hang the server with someone sending invalid class files that take forever to parse.
            CancellationTokenSource cts = new CancellationTokenSource(NapackClassAnalyzer.MaxParseTime);

            try
            {
                SyntaxTree tree = CSharpSyntaxTree.ParseText(contents, NapackClassAnalyzer.ParseOptions, string.Empty, Encoding.Default, cts.Token);
                return AnalyzeSyntaxTree(napackName, filename, tree).GetAwaiter().GetResult();
            }
            catch (Exception ex) when (
                ex.GetType() != typeof(UnsupportedNapackFileException) && 
                ex.GetType() != typeof(InvalidNapackFileException))
            {
                throw new InvalidNapackFileException(filename, ex.GetType().ToString() + ": " + ex.Message);
            }
        }

        internal static async Task<ICollection<ClassSpec>> AnalyzeSyntaxTree(string napackName, string filename, SyntaxTree tree)
        {
            SyntaxNode root = await tree.GetRootAsync();
            
            if (root.ChildNodes().Count(node => node.IsKind(SyntaxKind.NamespaceDeclaration)) > 1)
            {
                throw new UnsupportedNapackFileException(filename, "multiple namespace");
            }
            else if (!root.ChildNodes().Any(node => node.IsKind(SyntaxKind.NamespaceDeclaration)))
            {
                throw new InvalidNapackFileException(filename, "Missing a namespace!");
            }

            // Switch to the namespace as the root and verify (CASE SENSITIVE)
            root = root.ChildNodes().SingleOrDefault(node => node.IsKind(SyntaxKind.NamespaceDeclaration));
            if (!(root as NamespaceDeclarationSyntax).Name.ToFullString().Trim().Equals(napackName, StringComparison.InvariantCulture))
            {
                throw new InvalidNapackFileException(filename, "Namespace name is not " + napackName);
            }

            List<ClassSpec> parsedPublicClasses = new List<ClassSpec>();
            foreach (ClassDeclarationSyntax classNode in root.ChildNodes().Where(node => node.IsKind(SyntaxKind.ClassDeclaration)))
            {
                if (classNode.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)))
                {
                    parsedPublicClasses.Add(AnalyzeClassSyntaxTree(napackName, filename, classNode));
                }
            }

            return parsedPublicClasses;
        }

        internal static ClassSpec AnalyzeClassSyntaxTree(string napackName, string filename, ClassDeclarationSyntax classNode)
        {
            ClassSpec classSpec = new ClassSpec();
            classSpec.Name = DocumentedElement.LoadFromSyntaxNode(classNode);
            classSpec.IsAbstract = classNode.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.AbstractKeyword));
            classSpec.IsStatic = classNode.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword));
            classSpec.IsSealed = classSpec.IsStatic || classNode.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.SealedKeyword));

            // Parse classes
            foreach (ClassDeclarationSyntax node in classNode.ChildNodes().Where(node => node.IsKind(SyntaxKind.ClassDeclaration)))
            {
                if (node.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)) ||
                    (classSpec.ProtectedItemsConsideredPublic && node.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ProtectedKeyword))))
                {
                    // This recursion will exit because we aren't *compiling* the code, but merely parsing it.
                    classSpec.PublicClasses.Add(AnalyzeClassSyntaxTree(napackName, filename, node));
                }
            }

            // Parse methods
            foreach (MethodDeclarationSyntax node in classNode.ChildNodes().Where(node => node.IsKind(SyntaxKind.MethodDeclaration)))
            {
                if (node.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.PublicKeyword)) ||
                    (classSpec.ProtectedItemsConsideredPublic && node.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ProtectedKeyword))))
                {
                    classSpec.PublicMethods.Add(MethodSpec.LoadFromSyntaxNode(node));
                }
            }

            return classSpec;
        }
    }
}