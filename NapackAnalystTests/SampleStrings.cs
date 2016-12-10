namespace NapackAnalystTests
{
    public static class SampleStrings
    {
        public const string MultiNamespaceClass =
@"using System;
using System.Collections.Generic;
using System.IO;

namespace First
{
    class Fake
    {
    }
}

namespace Second
{
    class Fake
    {
    }
}";

        public const string NoNamespaceClass =
@"using System;
using System.Collections.Generic;
using System.IO;

class First
{
}
";

        public const string LargeSampleClass =
@"using System;
using System.Collections.Generic;
using System.IO;

namespace Test
{
    /// <summary>
    /// Documentation goes here.
    /// </summary>
    public class First
    {
        /// <summary>
        /// Summary goes here.
        /// </summary>\r\n" +
"       /// <param name=\"node\">Node goes here.</param>\r\n" +
"       /// <param name=\"test\">Has no modifiers</param>\n" +
"       /// <param name=\"result\">Result goes here.</param>\r" +
@"      /// <returns>DocumentedElement item.</returns>
        public DocumentedElement LoadFromSyntaxNode(ref ClassDeclarationSyntax node, int test, out int value)
        {
            // Random stuff exists here.
            SyntaxToken syntaxToken = node.ChildTokens().First(token => token.IsKind(SyntaxKind.IdentifierToken));
            return DocumentedElement.LoadFromSyntaxTokenAndTrivia(syntaxToken, node.GetLeadingTrivia());
        }
    }
}
";

    }
}
