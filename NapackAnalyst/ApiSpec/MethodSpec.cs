using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines a method specification. Any changes in here are breaking changes.
    /// </summary>
    public class MethodSpec
    {
        public MethodSpec()
        {
            this.Parameters = new List<ParameterSpec>();
        }

        public DocumentedElement Name { get; set; }

        public List<ParameterSpec> Parameters { get; set; }

        public string ReturnType { get; set; }

        internal static MethodSpec LoadFromSyntaxNode(MethodDeclarationSyntax node)
        {
            MethodSpec methodSpec = new MethodSpec();
            foreach (ParameterSyntax parameter in node.ParameterList.Parameters)
            {
                methodSpec.Parameters.Add(ParameterSpec.LoadFromSyntaxNode(parameter));
            }

            methodSpec.Name = DocumentedElement.LoadFromSyntaxNode(node);
            methodSpec.ReturnType = node.ReturnType.ToString();
            return methodSpec;
        }
    }
}