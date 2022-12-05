namespace Walgelijk
{
    /// <summary>
    /// Base class for attributes that indicate a component relationship
    /// </summary>
    public abstract class ComponentRelationAttribute : global::System.Attribute
    {
        /// <summary>
        /// Relevant component types
        /// </summary>
        public global::System.Type[]? Types { get; protected set; }
    }
}
