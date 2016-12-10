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

        public const string DocumentationTest =
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
    }
}
";

    }
}
