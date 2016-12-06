namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines the specification of a field. Any changes here are breaking changes.
    /// </summary>
    /// <remarks>
    /// This is a hole in the current Napack design -- property values can be modified by other methods, returning different values, potentially breaking consumers.
    /// However, the Napack design cannot prevent *all* breaking changes -- it just attempts to make breaking changes much harder and very explicit. 
    /// </remarks>
    public class PropertySpec
    {
        public DocumentedElement Name { get; set; }

        public bool IsStatic { get; set; }
    }
}