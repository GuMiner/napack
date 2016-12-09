namespace Napack.Analyst.ApiSpec
{
    /// <summary>
    /// Defines the publically-facing API of a single Napack class.
    /// </summary>
    public class NapackClassSpec
    {
        public NapackClassSpec()
        {
            this.PublicFields = new FieldSpec();
            this.PublicProperties = new PropertySpec();
            this.PublicConstructors = new ConstructorSpec();
            this.PublicFunctions = new FunctionSpec();
        }

        /// <summary>
        /// The class name and documentation associated with the class.
        /// </summary>
        public DocumentedElement Name { get; set; }

        public FieldSpec PublicFields { get; set; }

        public PropertySpec PublicProperties { get; set; }

        public ConstructorSpec PublicConstructors { get; set; }

        public FunctionSpec PublicFunctions { get; set; }

        /// <summary>
        /// Defines if a class is abstract or not. A change from a class *to* abstract is a breaking change, but *from* abstract is not.
        /// </summary>
        public bool IsAbstract { get; set; }
        
        /// <summary>
        /// Defines if a class is sealed or not. A change from a class *to* sealed is a breaking change, but *from* sealed is not.
        /// </summary>
        public bool IsSealed { get; set; }
        
        /// <summary>
        /// Defines if a class is static or not. Changing this field is a breaking change.
        /// </summary>
        public bool IsStatic { get; set; }
    }
}