using System.Collections.Generic;
using Newtonsoft.Json;

namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines the publically-facing API of a single Napack class.
    /// </summary>
    /// <remarks>
    /// Within a non-sealed class, a protected item will be considered public also.
    /// </remarks>
    public class ClassSpec
    {
        public ClassSpec()
        {
            this.PublicFields = new List<FieldSpec>();
            this.PublicProperties = new List<PropertySpec>();
            this.PublicConstructors = new List<ConstructorSpec>();
            this.PublicMethods = new List<MethodSpec>();
            this.PublicClasses = new List<ClassSpec>();
        }

        /// <summary>
        /// The class name and documentation associated with the class.
        /// </summary>
        public DocumentedElement Name { get; set; }

        public IList<FieldSpec> PublicFields { get; set; }

        public IList<PropertySpec> PublicProperties { get; set; }

        public IList<ConstructorSpec> PublicConstructors { get; set; }

        public IList<MethodSpec> PublicMethods { get; set; }

        public IList<ClassSpec> PublicClasses { get; set; }

        /// <summary>
        /// Defines if a class is abstract or not. A change from a class *to* abstract is a breaking change, but *from* abstract is not.
        /// </summary>
        public bool IsAbstract { get; set; }
        
        /// <summary>
        /// Defines if a class is static or not. Changing this field is a breaking change.
        /// </summary>
        public bool IsStatic { get; set; }

        /// <summary>
        /// Defines if a class is sealed or not. A change from a class *to* sealed is a breaking change, but *from* sealed is not.
        /// </summary>
        /// <remarks>
        /// A static class is by default a sealed class.
        /// </remarks>
        public bool IsSealed { get; set; }

        /// <summary>
        /// If true, protected items are considered public and can be found within the Public* fields.
        /// </summary>
        [JsonIgnore]
        public bool ProtectedItemsConsideredPublic => !this.IsStatic && this.IsSealed;

        
    }
}