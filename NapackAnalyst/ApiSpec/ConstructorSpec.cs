using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines a constructor specification. Any changes here are breaking changes.
    /// </summary>
    public class ConstructorSpec
    {
        public ConstructorSpec()
        {
            this.Parameters = new List<ParameterSpec>();
        }

        public DocumentedElement Name { get; set; }

        public List<ParameterSpec> Parameters { get; set; }

        internal static ConstructorSpec LoadFromSyntaxNode(ConstructorDeclarationSyntax node)
        {
            ConstructorSpec constructorSpec = new ConstructorSpec();
            foreach (ParameterSyntax parameter in node.ParameterList.Parameters)
            {
                constructorSpec.Parameters.Add(ParameterSpec.LoadFromSyntaxNode(parameter));
            }

            constructorSpec.Name = DocumentedElement.LoadFromSyntaxNode(node);
            return constructorSpec;
        }
    }
}