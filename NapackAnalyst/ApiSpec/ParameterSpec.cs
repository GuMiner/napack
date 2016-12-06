namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines a function or constructor parameter specification. Any changes in here are breaking changes.
    /// </summary>
    public class ParameterSpec
    {
        public string Modifier { get; set; }

        public string Type { get; set; }
        
        public string Name { get; set; }
    }
}