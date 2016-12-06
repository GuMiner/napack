using System.Collections.Generic;

namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines a constructor specification. Any changes here are breaking changes.
    /// </summary>
    public class ConstructorSpec
    {
        public DocumentedElement Name { get; set; }

        public List<ParameterSpec> Parameters { get; set; }
    }
}