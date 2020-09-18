namespace Walgelijk
{
    /// <summary>
    /// Attribute that lets the <see cref="Scene"/> know that something needs something else.
    /// </summary>
    public sealed class RequiresComponents : ComponentRelationAttribute
    {
        /// <summary>
        /// Construct a <see cref="RequiresComponents"/> with the given types
        /// </summary>
        /// <param name="componentTypes"></param>
        public RequiresComponents(params global::System.Type[] componentTypes)
        {
            Types = componentTypes;
        }
    }
}
