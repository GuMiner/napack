using System.Collections.Generic;

namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines the publically-facing API of a single Napack interface.
    /// </summary>
    public class InterfaceSpec
    {
        public InterfaceSpec()
        {
            this.Properties = new List<PropertySpec>();
            this.Methods = new List<MethodSpec>();
        }

        /// <summary>
        /// The interface name and documentation associated with the interface.
        /// </summary>
        public DocumentedElement Name { get; set; }
        
        /// <summary>
        /// The properties on the interface
        /// </summary>
        public IList<PropertySpec> Properties { get; set; }
        
        /// <summary>
        /// The methods on the interface
        /// </summary>
        public IList<MethodSpec> Methods { get; set; }        
    }
}