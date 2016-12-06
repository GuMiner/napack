using System.Collections.Generic;

namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines a function specification. Any changes in here are breaking changes.
    /// </summary>
    public class FunctionSpec
    {
        public DocumentedElement Name { get; set; }

        public List<ParameterSpec> Parameters { get; set; }
    }
}