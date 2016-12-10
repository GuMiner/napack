using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines the specification of a property. Any changes here are breaking changes.
    /// </summary>
    /// <remarks>
    /// This is a hole in the current Napack design -- property values can be modified by other methods, returning different values, potentially breaking consumers.
    /// However, the Napack design cannot prevent *all* breaking changes -- it just attempts to make breaking changes much harder and very explicit. 
    /// </remarks>
    public class PropertySpec
    {
        public DocumentedElement Name { get; set; }

        public string Type { get; set; }

        public bool IsStatic { get; set; }

        public static PropertySpec LoadFromSyntaxNode(PropertyDeclarationSyntax node)
        {
            PropertySpec propertySpec = new PropertySpec();
            propertySpec.IsStatic = node.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword));
            propertySpec.Type = node.Type.ToString();
            propertySpec.Name = DocumentedElement.LoadFromSyntaxNode(node);
            return propertySpec;
        }
    }
}