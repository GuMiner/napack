﻿namespace NapackAnalystTests
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
        private string NotReadField;

        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField = 22;
        
        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int alpha = 2)
        {
            // Random stuff exists here.
        }

        private PrivateProperty Test { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement Name { get; set; }

        /// <summary>
        /// Method summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"node\">Node goes here.</param>\r\n" +
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

        #region MinorChanges
        public const string LargeSampleMinorPropertyChange =
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
        private string NotReadField;

        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField = 22;
        
        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int alpha = 2)
        {
            // Random stuff exists here.
        }

        private PrivateProperty Test { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement Name2 { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement Name { get; set; }

        /// <summary>
        /// Method summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"node\">Node goes here.</param>\r\n" +
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

        public const string LargeSampleMinorFieldChange =
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
        private string NotReadField;

        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField = 22;
        
        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField2;

        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int alpha = 2)
        {
            // Random stuff exists here.
        }

        private PrivateProperty Test { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement Name { get; set; }

        /// <summary>
        /// Method summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"node\">Node goes here.</param>\r\n" +
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

        public const string LargeSampleMinorConstructorChange =
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
        private string NotReadField;

        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField = 22;
        
        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField2;

        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int alpha = 2)
        {
            // Random stuff exists here.
        }

        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int gamma, int alpha = 2)
        {
            // Random stuff exists here.
        }

        private PrivateProperty Test { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement Name { get; set; }

        /// <summary>
        /// Method summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"node\">Node goes here.</param>\r\n" +
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

        public const string LargeSampleMinorMethodChange =
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
        private string NotReadField;

        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField = 22;
        
        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField2;

        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int alpha = 2)
        {
            // Random stuff exists here.
        }

        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int gamma, int alpha = 2)
        {
            // Random stuff exists here.
        }

        private PrivateProperty Test { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement Name { get; set; }

        /// <summary>
        /// Method summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"node\">Node goes here.</param>\r\n" +
"       /// <param name=\"test\">Has no modifiers</param>\n" +
"       /// <param name=\"result\">Result goes here.</param>\r" +
@"      /// <returns>DocumentedElement item.</returns>
        public DocumentedElement LoadFromSyntaxNode(ref ClassDeclarationSyntax node, int test, out int value)
        {
            // Random stuff exists here.
            SyntaxToken syntaxToken = node.ChildTokens().First(token => token.IsKind(SyntaxKind.IdentifierToken));
            return DocumentedElement.LoadFromSyntaxTokenAndTrivia(syntaxToken, node.GetLeadingTrivia());
        }

        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
";
        #endregion

        #region MajorChanges
        public const string LargeSampleMajorPropertyChange =
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
        private string NotReadField;

        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField = 22;
        
        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int alpha = 2)
        {
            // Random stuff exists here.
        }

        private PrivateProperty Test { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement Name2 { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement NameIsWrong { get; set; }

        /// <summary>
        /// Method summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"node\">Node goes here.</param>\r\n" +
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

        public const string LargeSampleMajorFieldChange =
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
        private string NotReadField;

        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstatField = 22;
        
        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField2;

        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int alpha = 2)
        {
            // Random stuff exists here.
        }

        private PrivateProperty Test { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement Name { get; set; }

        /// <summary>
        /// Method summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"node\">Node goes here.</param>\r\n" +
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

        public const string LargeSampleMajorConstructorChange =
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
        private string NotReadField;

        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField = 22;
        
        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField2;

        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int betaa, int alpha = 2)
        {
            // Random stuff exists here.
        }

        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int gamma, int alpha = 2)
        {
            // Random stuff exists here.
        }

        private PrivateProperty Test { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement Name { get; set; }

        /// <summary>
        /// Method summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"node\">Node goes here.</param>\r\n" +
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

        public const string LargeSampleMajorMethodChange =
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
        private string NotReadField;

        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField = 22;
        
        /// <summary>
        /// Field summary goes here.
        /// </summary>
        public const string ConstField2;

        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int alpha = 2)
        {
            // Random stuff exists here.
        }

        /// <summary>
        /// Constructor summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"alpha\"></param>\r\n" +
"       /// <param name=\"beta\"></param>\r\n" +
@"      public First(ref int beta, int gamma, int alpha = 2)
        {
            // Random stuff exists here.
        }

        private PrivateProperty Test { get; set; }

        /// <summary>
        /// Property documentation goes here.
        /// </summary>
        public DocumentedElement Name { get; set; }

        /// <summary>
        /// Method summary goes here.
        /// </summary>" +
"\r\n   /// <param name=\"node\">Node goes here.</param>\r\n" +
"       /// <param name=\"test\">Has no modifiers</param>\n" +
"       /// <param name=\"result\">Result goes here.</param>\r" +
@"      /// <returns>DocumentedElement item.</returns>
        public DocumentedElement LoadFromSyntaxNode(ref ClassDeclarationSyntax node3, int test, out int value)
        {
            // Random stuff exists here.
            SyntaxToken syntaxToken = node.ChildTokens().First(token => token.IsKind(SyntaxKind.IdentifierToken));
            return DocumentedElement.LoadFromSyntaxTokenAndTrivia(syntaxToken, node.GetLeadingTrivia());
        }

        public int Add(int a, int b)
        {
            return a + b;
        }
    }
}
";
        #endregion
    }
}
