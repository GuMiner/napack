using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines a documented element used for automatic API generation within the Napack system.
    /// </summary>
    public class DocumentedElement
    {
        public string Name { get; set; }

        public List<string> Documentation { get; set; }
        
        public static DocumentedElement LoadFromSyntaxNode(ClassDeclarationSyntax node)
        {
            SyntaxToken syntaxToken = node.ChildTokens().First(token => token.IsKind(SyntaxKind.IdentifierToken));
            return DocumentedElement.LoadFromSyntaxTokenAndTrivia(syntaxToken, node.GetLeadingTrivia());
        }

        public static DocumentedElement LoadFromSyntaxNode(MethodDeclarationSyntax node)
        {
            SyntaxToken syntaxToken = node.ChildTokens().First(token => token.IsKind(SyntaxKind.IdentifierToken));
            return DocumentedElement.LoadFromSyntaxTokenAndTrivia(syntaxToken, node.GetLeadingTrivia());
        }

        private static DocumentedElement LoadFromSyntaxTokenAndTrivia(SyntaxToken token, SyntaxTriviaList triviaList)
        {
            DocumentedElement element = new DocumentedElement();
            element.Name = token.Text;

            // Break apart lines that come as a single group, remove empty lines, and trim whitespace overall.
            element.Documentation = triviaList.SelectMany(line => line.ToFullString().Split(new[] { '\r', '\n' })).ToList();
            element.Documentation = element.Documentation
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim()).ToList();

            return element;
        }
    }
}