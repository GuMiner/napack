using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Newtonsoft.Json;

namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines the specification of a field. Any changes here are breaking changes.
    /// </summary>
    /// <remarks>
    /// This is a hole in the current Napack design -- field values can be modified by other methods, returning different values, potentially breaking consumers.
    /// However, the Napack design cannot prevent *all* breaking changes -- it just attempts to make breaking changes much harder and very explicit. 
    /// </remarks>
    public class FieldSpec
    {
        public DocumentedElement Name { get; set; }
        
        public string Type { get; set; }

        public bool IsConst { get; set; }

        public bool IsReadonly { get; set; }

        public bool IsStatic { get; set; }

        [JsonIgnore]
        public bool IsUserModifiable => !this.IsConst && !this.IsReadonly;

        public static FieldSpec LoadFromSyntaxNode(FieldDeclarationSyntax node)
        {
            FieldSpec fieldSpec = new FieldSpec();
            fieldSpec.IsConst = node.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ConstKeyword));
            fieldSpec.IsReadonly = node.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.ReadOnlyKeyword));
            fieldSpec.IsStatic = node.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword));

            // Null access will throw and be caught in our overall analyzer handler.
            VariableDeclarationSyntax variable = node.ChildNodes()
                .FirstOrDefault(childNode => childNode.IsKind(SyntaxKind.VariableDeclaration)) as VariableDeclarationSyntax;
            fieldSpec.Type = variable.Type.ToString();
            fieldSpec.Name = DocumentedElement.LoadFromSyntaxNode(variable, node.GetLeadingTrivia());
            return fieldSpec;
        }
    }
}